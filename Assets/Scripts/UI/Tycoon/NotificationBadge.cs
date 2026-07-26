using UnityEngine;
using UnityEngine.UI;

public enum NotificationType
{
    None = 0,
    New,
    HeroRequest,
}

public class NotificationBadge : MonoBehaviour
{
    [SerializeField] private GameObject GameObject_Badge;
    [SerializeField] private Image Image_Icon;

    [SerializeField] private Sprite Sprite_New;
    [SerializeField] private Color Color_New = new Color32(0xE7, 0xB9, 0x5C, 0xFF);
    
    [SerializeField] private Sprite Sprite_HeroRequest;

    public void SetNotification(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.None:
                GameObject_Badge.SetActive(false);
                break;

            case NotificationType.New:
                GameObject_Badge.SetActive(true);
                Image_Icon.sprite = Sprite_New;
                Image_Icon.color = Color_New;
                break;

            case NotificationType.HeroRequest:
                GameObject_Badge.SetActive(true);
                Image_Icon.sprite = Sprite_HeroRequest;
                break;
        }
    }
}