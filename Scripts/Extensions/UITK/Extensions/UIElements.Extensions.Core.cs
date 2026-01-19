#if UNITY_6000_0_OR_NEWER
using MyTools.UIElements;
using UnityEngine.UIElements;

public static partial class UIToolKitExtensions
{
        
    #region Basics
    
    public static T Name<T>(this T e, string value) where T : VisualElement => (e.name = value, e).Item2;
    
    #endregion
    
    #region Styling
    
    public static T StyleSheet<T>(this T e, StyleSheet value) where T : VisualElement
    {
        e.styleSheets.Add(value);
        return e;
    }
    
    public static T USS<T>(this T e, string A) where T : VisualElement
    {
        if (string.IsNullOrWhiteSpace(A)) return e;

        var classes = A.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var className in classes)
        {
            e.AddToClassList(className);
        }

        return e;
    }
    
    #endregion
    
    #region Parenting
    
    public static T Parent<T>(this T element, VisualElement parent)
        where T : VisualElement
    {
        parent.Add(element);
        return element;
    }
    
    #endregion
    
    #region Gap
    
    public static CustomVisualElement Gap(this CustomVisualElement e, int value) => (e.gapX = value, e.gapY = value, e).Item3;
    
    public static CustomVisualElement Gap(this CustomVisualElement e, int gapX, int gapY) => (e.gapX = gapX, e.gapY = gapY, e).Item3;
    
    public static CustomVisualElement GapX(this CustomVisualElement e, int value) => (e.gapX = value, e).Item2;
    
    public static CustomVisualElement GapY(this CustomVisualElement e, int value) => (e.gapY = value, e).Item2;
    
    #endregion
}
#endif