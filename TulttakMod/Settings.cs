using UnityModManagerNet;

namespace TulttakMod {
    public class MainSettings : UnityModManager.ModSettings, IDrawable {
        [Draw("[나 ] 키로 반대편 페이지로 가기 방지")] public bool EventNoMoveToFirst = false;
        [Draw("여러 타일 선택시 비트수 표시(from AdofaiUtils)")] public bool ShowBeats = false;
        [Draw("선택한 타일 각도 보기(from EditorHelper)")] public bool ShowAngle = false;

        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }
        
        public void OnChange() {
            
        }
        
        public void OnGUI(UnityModManager.ModEntry modEntry) {
            // GUILayout.Label("와 샌즈");
            // StringTest = GUILayout.TextField(StringTest);
            
            Main.Settings.Draw(modEntry);
        }

        public void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            Main.Settings.Save(modEntry);
        }
    }
}