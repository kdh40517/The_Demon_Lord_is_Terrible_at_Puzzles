namespace SeoAhn
{
    // 씬 이동 중에 "로딩씬이 다음에 어디로 가야 하는지" 임시로 저장하는 클래스입니다.
    // PlayerPrefs처럼 오래 저장되지 않고, 게임 실행 중에만 유지됩니다.
    public static class SceneTransitionData
    {
        // 로딩씬이 이동할 다음 씬 이름입니다.
        public static string NextSceneName = string.Empty;

        // 다음 씬 이름을 저장합니다.
        public static void SetNextScene(string sceneName)
        {
            NextSceneName = sceneName;
        }

        // 저장된 다음 씬 이름을 가져옵니다.
        public static string GetNextScene(string defaultSceneName)
        {
            if (string.IsNullOrEmpty(NextSceneName))
            {
                return defaultSceneName;
            }

            return NextSceneName;
        }

        // 한 번 사용한 다음 씬 이름을 비웁니다.
        public static void Clear()
        {
            NextSceneName = string.Empty;
        }
    }
}