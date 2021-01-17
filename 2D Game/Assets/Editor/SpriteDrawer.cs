using UnityEngine;
using UnityEditor;
using Game.Extensions;

namespace Game
{
    [CustomPropertyDrawer(typeof(Sprite))]
    public class SpriteDrawer : PropertyDrawer
    {
        private static readonly GUIStyle style = new GUIStyle();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var identLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            //create object field for the sprite
            var spriteRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.objectReferenceValue = EditorGUI.ObjectField(spriteRect, property.name.FirstCharToUpper(), property.objectReferenceValue, typeof(Sprite), false);

            if (Event.current.type != EventType.Repaint || property.objectReferenceValue == null)
                return;

            //draw a sprite
            var sprite = property.objectReferenceValue as Sprite;

            spriteRect.y += EditorGUIUtility.singleLineHeight + 4;
            spriteRect.width = 64;
            spriteRect.height = 64;

            style.normal.background = sprite.texture;
            style.Draw(spriteRect, GUIContent.none, false, false, false, false);

            EditorGUI.indentLevel = identLevel;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => base.GetPropertyHeight(property, label) + 70f;
    }
}
