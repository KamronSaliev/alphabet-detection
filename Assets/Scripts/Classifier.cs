using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Sentis;
using UnityEngine;

namespace AlphabetDetection
{
    public class Classifier : MonoBehaviour
    {
        [Header("Main")] [SerializeField] private Drawer _drawer;
        [SerializeField] private ModelAsset _modelAsset;
        [SerializeField] private string[] _results;

        [Header("View")] [SerializeField] private TMP_Text _text;

        private readonly Dictionary<int, int> _map = new()
        {
            { 0, 48 }, { 1, 49 }, { 2, 50 }, { 3, 51 }, { 4, 52 }, { 5, 53 }, { 6, 54 }, { 7, 55 }, { 8, 56 },
            { 9, 57 }, { 10, 65 }, { 11, 66 }, { 12, 67 }, { 13, 68 }, { 14, 69 }, { 15, 70 }, { 16, 71 }, { 17, 72 },
            { 18, 73 }, { 19, 74 }, { 20, 75 }, { 21, 76 }, { 22, 77 }, { 23, 78 }, { 24, 79 }, { 25, 80 }, { 26, 81 },
            { 27, 82 }, { 28, 83 }, { 29, 84 }, { 30, 85 }, { 31, 86 }, { 32, 87 }, { 33, 88 }, { 34, 89 }, { 35, 90 },
            { 36, 97 }, { 37, 98 }, { 38, 100 }, { 39, 101 }, { 40, 102 }, { 41, 103 }, { 42, 104 }, { 43, 110 },
            { 44, 113 }, { 45, 114 }, { 46, 116 }
        };

        private const string SoftmaxOutputName = "dense_2";

        private Model _runtimeModel;
        private IWorker _worker;
        private Tensor _inputTensor;
        private Texture2D _inputTexture;
        private TensorFloat _outputTensor;

        public void Classify()
        {
            var results = GetResults(_drawer.DrawingTexture);
            var max = results.Max();
            var index = results.ToList().IndexOf(max);

            Debug.Log($"Result: {(char)_map[index]}, {max}");

            View(results);
        }

        private void OnDestroy()
        {
            _inputTensor?.Dispose();
            _worker?.Dispose();
            _outputTensor?.Dispose();
        }

        private void View(float[] results)
        {
            _results = results.Select((value, index) => $"{(char)_map[index]}:\t{value:F2}").ToArray();
            _text.text = string.Join("\n", _results);
        }

        private float[] GetResults(Texture2D texture)
        {
            _inputTensor?.Dispose();
            _worker?.Dispose();
            _outputTensor?.Dispose();

            var softmaxOutputName = SoftmaxOutputName;
            _runtimeModel = ModelLoader.Load(_modelAsset);
            _runtimeModel.outputs[0] = softmaxOutputName;

            _inputTensor = TextureConverter.ToTensor(texture, 28, 28, 1);

            _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);
            _worker.Execute(_inputTensor);

            _outputTensor = _worker.PeekOutput() as TensorFloat;

            if (_outputTensor == null)
            {
                return new float[] { };
            }

            _outputTensor.MakeReadable();
            return _outputTensor.ToReadOnlyArray();
        }
    }
}