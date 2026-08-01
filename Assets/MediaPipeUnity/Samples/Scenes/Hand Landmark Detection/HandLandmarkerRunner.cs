using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    [System.Serializable]
    public class FingerTarget
    {
        public Transform target;
        public int fingerIndex;
    }

    public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
    {
        [SerializeField] public float multix = 185f;
        [SerializeField] public float multiy = 105f;

        [SerializeField]
        private FingerTarget[] fingers;

        [SerializeField]
        private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;

        private Experimental.TextureFramePool _textureFramePool;

        private readonly Vector3[] landmarkPositions = new Vector3[21];
        private bool handDetected;

        public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        private void Update()
        {
            if (!handDetected) return;

            foreach (var finger in fingers)
            {
                if (finger.target == null) continue;

                if (finger.fingerIndex >= 0 &&
                    finger.fingerIndex < landmarkPositions.Length)
                {
                    finger.target.position =
                        landmarkPositions[finger.fingerIndex];
                }
            }
        }

        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Image Read Mode = {config.ImageReadMode}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumHands = {config.NumHands}");
            Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
            Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetHandLandmarkerOptions(
                config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM
                ? OnHandLandmarkDetectionOutput
                : null);

            taskApi = HandLandmarker.CreateFromOptions(
                options,
                GpuManager.GpuResources);

            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            _textureFramePool = new Experimental.TextureFramePool(
                imageSource.textureWidth,
                imageSource.textureHeight,
                TextureFormat.RGBA32,
                10);

            screen.Initialize(imageSource);

            SetupAnnotationController(
                _handLandmarkerResultAnnotationController,
                imageSource);

            var transformationOptions =
                imageSource.GetTransformationOptions();

            var flipHorizontally =
                transformationOptions.flipHorizontally;

            var flipVertically =
                transformationOptions.flipVertically;

            var imageProcessingOptions =
                new Tasks.Vision.Core.ImageProcessingOptions(
                    rotationDegrees:
                    (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;

            var waitUntilReqDone =
                new WaitUntil(() => req.done);

            var waitForEndOfFrame =
                new WaitForEndOfFrame();

            var result =
                HandLandmarkerResult.Alloc(options.numHands);

            var canUseGpuImage =
                SystemInfo.graphicsDeviceType ==
                GraphicsDeviceType.OpenGLES3 &&
                GpuManager.GpuResources != null;

            using var glContext =
                canUseGpuImage
                ? GpuManager.GetGlContext()
                : null;

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                Image image;

                switch (config.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        textureFrame.ReadTextureOnGPU(
                            imageSource.GetCurrentTexture(),
                            flipHorizontally,
                            flipVertically);

                        image = textureFrame.BuildGPUImage(glContext);

                        yield return waitForEndOfFrame;
                        break;

                    case ImageReadMode.CPU:
                        yield return waitForEndOfFrame;

                        textureFrame.ReadTextureOnCPU(
                            imageSource.GetCurrentTexture(),
                            flipHorizontally,
                            flipVertically);

                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;

                    default:
                        req = textureFrame.ReadTextureAsync(
                            imageSource.GetCurrentTexture(),
                            flipHorizontally,
                            flipVertically);

                        yield return waitUntilReqDone;

                        if (req.hasError)
                        {
                            continue;
                        }

                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(
                            image,
                            imageProcessingOptions,
                            ref result))
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        break;

                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(
                            image,
                            GetCurrentTimestampMillisec(),
                            imageProcessingOptions,
                            ref result))
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        break;

                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(
                            image,
                            GetCurrentTimestampMillisec(),
                            imageProcessingOptions);
                        break;
                }
            }
        }

        private void OnHandLandmarkDetectionOutput(
            HandLandmarkerResult result,
            Image image,
            long timestamp)
        {
            if (result.handLandmarks != null &&
                result.handLandmarks.Count > 0)
            {
                for (int i = 0; i < result.handLandmarks[0].landmarks.Count; i++)
                {
                    var landmark =
                        result.handLandmarks[0].landmarks[i];

                    landmarkPositions[i] = new Vector3(
                        (landmark.x - 0.5f) * multix,
                        -(landmark.y - 0.5f) * multiy,
                        80f
                    );
                }

                handDetected = true;
            }

            _handLandmarkerResultAnnotationController.DrawLater(result);
        }
    }
}