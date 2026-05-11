namespace SeoAhn
{
    // 게임 실행 중에만 볼륨 값을 기억하는 클래스입니다.
    // PlayerPrefs를 사용하지 않기 때문에 게임을 완전히 껐다 켜면 다시 1로 초기화됩니다.
    public static class AudioVolumeData
    {
        // 전체 볼륨입니다.
        public static float MasterVolume = 1f;

        // 배경음 볼륨입니다.
        public static float BGMVolume = 1f;

        // 효과음 볼륨입니다.
        public static float SFXVolume = 1f;
    }
}