namespace SeoAhn
{
    // 게임 실행 중 스테이지 클리어 상태를 기억합니다.
    // 게임을 종료하면 자동 초기화됩니다.
    public static class StageClearManager
    {
        private static bool villageClear;
        private static bool forestClear;
        private static bool castleClear;
        private static bool devillClear;

        private static string recentlyClearedStage = string.Empty;

        public static void SetVillageClear()
        {
            villageClear = true;
            recentlyClearedStage = "Village";
        }

        public static void SetForestClear()
        {
            forestClear = true;
            recentlyClearedStage = "Forest";
        }

        public static void SetCastleClear()
        {
            castleClear = true;
            recentlyClearedStage = "Castle";
        }

        public static void SetDevillClear()
        {
            devillClear = true;
            recentlyClearedStage = "Devill";
        }

        public static bool IsVillageClear()
        {
            return villageClear;
        }

        public static bool IsForestClear()
        {
            return forestClear;
        }

        public static bool IsCastleClear()
        {
            return castleClear;
        }

        public static bool IsDevillClear()
        {
            return devillClear;
        }

        public static string GetRecentlyClearedStage()
        {
            return recentlyClearedStage;
        }

        public static void ClearRecentlyClearedStage()
        {
            recentlyClearedStage = string.Empty;
        }

        public static void ResetAll()
        {
            villageClear = false;
            forestClear = false;
            castleClear = false;
            devillClear = false;
            recentlyClearedStage = string.Empty;
        }
    }
}