using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameCharacter;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private Button nextMessageBtn;

    public Queue<DialogueLine> dialogueLines = new();

    public bool isDialogueActive = false;
    public float typingSpeed = 0.1f;

    public Animator dialogueAnim;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        nextMessageBtn.onClick.AddListener(DisplayNextDialogueLine);
    }

    private void OnDestroy()
    {
        nextMessageBtn.onClick.RemoveAllListeners();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueContainer.SetActive(true);
        isDialogueActive = true;
        dialogueAnim.Play("Appear");
        //StartCoroutine(DelayDialogue());
        dialogueLines.Clear();
        foreach (var dialogueLine in dialogue.dialogueLines)
        {
            dialogueLines.Enqueue(dialogueLine);
        }
        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if(dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = dialogueLines.Dequeue();
        characterImage.sprite = currentLine.dialogueCharacter.icon;
        nameCharacter.text = currentLine.dialogueCharacter.name;

        StopAllCoroutines();
        StartCoroutine(TypeSentences(currentLine));
    }

    IEnumerator TypeSentences(DialogueLine dialogueLine)
    {
        message.text = string.Empty;
        foreach (var text in dialogueLine.text)
        {
            message.text += text;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void EndDialogue()
    {
        dialogueAnim.Play("Disappear");
        StartCoroutine(DelayHideDialogue());
        isDialogueActive = false;
    }

    IEnumerator DelayHideDialogue()
    {
        yield return new WaitForSeconds(0.5f);
        dialogueContainer.SetActive(false);
    }

}
