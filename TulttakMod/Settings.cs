using UnityModManagerNet;

namespace TulttakMod {
    public class MainSettings : UnityModManager.ModSettings, IDrawable {
        [Draw("[나 ] 키로 반대편 페이지로 가기 방지")] public bool EventNoMoveToFirst = false;
        [Draw("여러 타일 선택시 비트수 표시(from AdofaiUtils)")] public bool ShowBeats = false;
        [Draw("선택한 타일 각도 보기(from EditorHelper)")] public bool ShowAngle = false;
        
        [Draw("이전 타일 자동 소용돌이 설치")] public bool AutoTwirl = false;
        [Draw("동타 두번째 타일 제외", VisibleOn = "AutoTwirl|true")] public bool ExcludePseudoSecondFloor = false;
        [Draw("동타 최대 각도", VisibleOn = "ExcludePseudoSecondFloor|true", Min = 0f, Max = 360f)] public float PseudoMaxAngle = 30f;
        [Draw("자동 소용돌이 최소 각도", VisibleOn = "AutoTwirl|true", Min = 0f, Max = 360f)] public float TwirlMinAngle = 181f;
        [Draw("자동 소용돌이 최대 각도", VisibleOn = "AutoTwirl|true", Min = 0f, Max = 360f)] public float TwirlMaxAngle = 359f;

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