using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

namespace AlphabetDetection
{
    public class Classifier : MonoBehaviour
    {
        [Header("Main")] [SerializeField] private Texture2D _inputTexture;
        [SerializeField] private ModelAsset _modelAsset;
        [SerializeField] private string[] _results;

        [Header("View")] [SerializeField] private RawImage _image;
        [SerializeField] private TMP_Text _text;

        private const string SoftmaxOutputName = "dense_2";

        private readonly Dictionary<int, int> _map = new()
        {
            { 0, 48 }, { 1, 49 }, { 2, 50 }, { 3, 51 }, { 4, 52 }, { 5, 53 }, { 6, 54 }, { 7, 55 }, { 8, 56 },
            { 9, 57 }, { 10, 65 }, { 11, 66 }, { 12, 67 }, { 13, 68 }, { 14, 69 }, { 15, 70 }, { 16, 71 }, { 17, 72 },
            { 18, 73 }, { 19, 74 }, { 20, 75 }, { 21, 76 }, { 22, 77 }, { 23, 78 }, { 24, 79 }, { 25, 80 }, { 26, 81 },
            { 27, 82 }, { 28, 83 }, { 29, 84 }, { 30, 85 }, { 31, 86 }, { 32, 87 }, { 33, 88 }, { 34, 89 }, { 35, 90 },
            { 36, 97 }, { 37, 98 }, { 38, 100 }, { 39, 101 }, { 40, 102 }, { 41, 103 }, { 42, 104 }, { 43, 110 },
            { 44, 113 }, { 45, 114 }, { 46, 116 }
        };

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
            Debug.LogWarning($"{string.Join(" ", results)}");

            _results = results.Select((value, index) => $"{(char)_map[index]}:\t{value:F2}").ToArray();

            _image.texture = _inputTexture;
            _text.text = string.Join("\n", _results);
        }

        private float[] GetResults()
        {
            var softmaxOutputName = SoftmaxOutputName;
            _runtimeModel = ModelLoader.Load(_modelAsset);

            // _runtimeModel.AddLayer(new Softmax(softmaxOutputName, _runtimeModel.outputs[0]));

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