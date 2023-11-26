using System;
using System.Collections;
using GridModule;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject _levelCompleteUI;

    [SerializeField] 
    private Button _nextLevelButton;

    [SerializeField] 
    private LevelBuilder _levelBuilder;

    public static event Action<bool> PlayStateChangedEvent; 
    
    private void Start()
    {
        ListenEvents();
        _levelBuilder.InitBoard();
        PlayStateChangedEvent?.Invoke(true);
    }

    private void ListenEvents()
    {
        GridController.GridCompletedEvent += OnGridCompleted;
        _nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
    }

    private void OnGridCompleted()
    {
        StartCoroutine(LevelComplete());
    }
    
    private IEnumerator LevelComplete()
    {
        PlayStateChangedEvent?.Invoke(false);
        yield return new WaitForSeconds(1f);
        _levelCompleteUI.SetActive(true);
    }
    
    private void OnNextLevelButtonClicked()
    {
        _levelCompleteUI.SetActive(false);
        _levelBuilder.InitBoard();
        PlayStateChangedEvent?.Invoke(true);
    }
}