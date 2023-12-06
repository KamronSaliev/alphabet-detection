using System.Linq;
using TMPro;
using Unity.Sentis;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.UI;

namespace AlphabetDetection
{
    public class DigitDetector : MonoBehaviour
    {
        [Header("Main")] [SerializeField] private Texture2D _inputTexture;
        [SerializeField] private ModelAsset _modelAsset;

        [Header("View")] [SerializeField] private RawImage _image;
        [SerializeField] private TMP_Text _text;

        private const string SoftmaxOutputName = "Softmax_Output";

        private Model _runtimeModel;
        private IWorker _worker;

        private void Start()
        {
            var results = GetResults();

            View(results);
        }

        private void OnDisable()
        {
            _worker.Dispose();
        }

        private void View(float[] results)
        {
            var resultsWithIndex = results.Select((value, index) => $"{index}:\t{value:F2}").ToArray();
            
            _image.texture = _inputTexture;
            _text.text = string.Join("\n", resultsWithIndex);
        }

        private float[] GetResults()
        {
            var softmaxOutputName = SoftmaxOutputName;
            _runtimeModel = ModelLoader.Load(_modelAsset);
            _runtimeModel.AddLayer(new Softmax(softmaxOutputName, _runtimeModel.outputs[0]));
            _runtimeModel.outputs[0] = softmaxOutputName;

            using Tensor inputTensor = TextureConverter.ToTensor(_inputTexture, 28, 28, 1);

            _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);
            _worker.Execute(inputTensor);

            using var outputTensor = _worker.PeekOutput() as TensorFloat;

            if (outputTensor == null)
            {
                return new float[] { };
            }

            outputTensor.MakeReadable();
            return outputTensor.ToReadOnlyArray();
        }
    }
}