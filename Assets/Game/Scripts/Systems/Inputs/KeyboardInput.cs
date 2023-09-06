using UnityEngine;

namespace KartGame.KartSystems {

    public class KeyboardInput : BaseInput
    {
        public string TurnInputName = "Horizontal";
        public string AccelerateButtonName = "Accelerate";
        public string BrakeButtonName = "Brake";
        public string Jump = "Jump";
        public string SwitchName = "switch";
        public string Fire = "Fire1";
        public override InputData GenerateInput() {
            return new InputData
            {
                Jump = Input.GetButton(Jump),
                Accelerate = Input.GetButton(AccelerateButtonName),
                Brake = Input.GetButton(BrakeButtonName),
                SwitchGravity = Input.GetButton(SwitchName),
                TurnInput = Input.GetAxis("Horizontal"),
                Fire = Input.GetButton(Fire)                
            };
        }
    }
}
