namespace PMP {
    public class PlanarReflectionResolutionUpdater {

        PlanarReflectionHandler prHandler;

        public PlanarReflectionResolutionUpdater(PlanarReflectionHandler handler) {
            prHandler = handler;
        }

        float currentMul, lastMul = 0;

        public void Check() {
            if (!prHandler) return;

            currentMul = prHandler.GetResolutionMultiplier();
            if (currentMul != lastMul) {
                lastMul = currentMul;
                prHandler.ResizeRenderTexture(currentMul);
            }
        }
    }
}