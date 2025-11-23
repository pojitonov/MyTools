#if UNITY_EDITOR
namespace MyTools
{
    public static class Menus
    {
        public const string MY_TOOLS_MENU = "MyTools/";
        
        public const int GLOBAL_INDEX = 10000;
        public const string GLOBAL_MENU = MY_TOOLS_MENU + "Global/";
        
        public const int OBJECT_INDEX = 20000;
        public const string OBJECT_MENU = MY_TOOLS_MENU + "Objects/";
        public const int ASSETS_INDEX = 20001;
        public const string ASSETS_MENU = MY_TOOLS_MENU + "Assets/";
        
        public const int EDITOR_INDEX = 30000;
        public const string EDITOR_MENU = MY_TOOLS_MENU + "Editor/";
        public const int EDITOR_UI_INDEX = 30001;
        public const string EDITOR_UI_MENU = MY_TOOLS_MENU + "UI Toolkit/";
        
        public const int SCENE_VIEW_INDEX = 40000;
        public const string NAVIGATION_MENU = MY_TOOLS_MENU + "Scene View Navigation/";
        public const string BOOKMARKS_MENU = MY_TOOLS_MENU + "Scene View Bookmarks/";
        public const string TOOLS_MENU = MY_TOOLS_MENU + "Scene View Tools/";
        
        public const int SELECTION_INDEX = 50000;
        public const string SELECTION_MENU = MY_TOOLS_MENU + "Selection Groups/";

        public const int MODES_INDEX = 99000;
    }
}
#endif