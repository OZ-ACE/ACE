using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private Transform Transform_Background;
    [SerializeField] private Transform Transform_Speaker;

    private DialogueViewModel _dialogueVM;

    private GameObject _currentBackground;
    private GameObject _currentSpeaker;
    private string _currentBackgroundName;
    private string _currentSpeakerName;

    private Dictionary<string, GameObject> _backgrounds = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _characters = new Dictionary<string, GameObject>();

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        Image_Background.gameObject.SetActive(false);
    }

    public void Init(DialogueViewModel dialgoueVM)
    {
        if (_dialogueVM != null)
        {
            _dialogueVM.PropertyChanged -= OnPropertyChanged_View;
        }

        _dialogueVM = dialgoueVM;
        _dialogueVM.PropertyChanged += OnPropertyChanged_View;

        UpdateEnvironment();
    }

    private void OnDestroy()
    {
        if (_dialogueVM != null)
        {
            _dialogueVM.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DialogueViewModel.Background):
                ChangeBackground(_dialogueVM.Background).Forget();
                break;

            case nameof(DialogueViewModel.Speaker):
                ChangeCharacter(_dialogueVM.Speaker).Forget();
                break;
        }
    }

    private void UpdateEnvironment()
    {
        ChangeBackground(_dialogueVM.Background).Forget();
        ChangeCharacter(_dialogueVM.Speaker).Forget();
    }

    private async UniTask ChangeBackground(string name)
    {
        if (_currentBackgroundName == name || string.IsNullOrEmpty(name))
        {
            return;
        }

        if (_currentBackground != null)
        {
            _currentBackground.SetActive(false);
            Image_Background.gameObject.SetActive(false);
        }

        if (!name.Contains("Room"))
        {
            Image_Background.gameObject.SetActive(true);
            Image_Background.sprite = await ResourceManager.Inst.LoadSprite($"Image/{name}");

            return;
        }

        Debug.Log(name);

        if (!_backgrounds.ContainsKey(name))
        {
            string path = $"Prefabs/Room/{GameDataManager.Inst.GetData<Dialogue>(GameManager.Inst.CurrentDialogueID).Background}";
            GameObject prefab = await ResourceManager.Inst.InstantiateAsync(path, Transform_Background);
            _backgrounds.Add(name, prefab);
        }
        else
        {
            _backgrounds[name].SetActive(true);
        }

        _currentBackgroundName = name;
        _currentBackground = _backgrounds[name];
    }

    private async UniTask ChangeCharacter(string speaker)
    {
        if (_currentSpeakerName == speaker)
        {
            return;
        }

        if (string.IsNullOrEmpty(speaker) || _currentSpeaker != null)
        {
            _currentSpeaker.SetActive(false);
        }

        if (!_characters.ContainsKey(speaker))
        {
            string path = GameDataManager.Inst.GetData<Dialogue>(GameManager.Inst.CurrentDialogueID).SpeakerPath;
            GameObject prefab;

            if (path.Contains("hero"))
            {
                prefab = await ResourceManager.Inst.InstantiateAsync(path, Transform_Speaker);
                prefab.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                prefab.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            }
            else
            {
                prefab = await ResourceManager.Inst.InstantiateAsync(path, Transform_Speaker);
            }

            _characters.Add(speaker, prefab);
        }
        else
        {
            _characters[speaker].SetActive(true);
        }

        _currentSpeakerName = speaker;
        _currentSpeaker = _characters[speaker];
    }
}