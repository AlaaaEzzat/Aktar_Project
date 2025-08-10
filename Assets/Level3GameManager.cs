using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level3GameManager : GameManager
{
    public List<GameObject> CorrectItems;
    public LineTween Line;
    public List<GameObject> iconItems;
    public int CurrentCount = 0;
    public int CurrentPurchesedItems = 0;
    public TextMeshProUGUI countUiText;
    public GameObject itemToBuy;
    public int currentItemPrice = 0;
    public Dictionary<GameObject, int> itemsPurchesed = new Dictionary<GameObject, int>();
    public Dictionary<GameObject, int> itemsPrice = new Dictionary<GameObject, int>();
    public GameObject FinalHolder;
    public GameObject FinalCount;
    public List<Button> answerButtons;
    public override void OnItemSelected(SelectableItem item)
    {
        if (gameFinished) return;
        if (item.isCorrect)
        {
            correctChoices++;
            item.GetComponent<Image>().DOFade(1f, 0.2f).OnComplete(() =>
            {
                item.transform.DOPunchScale(new Vector3(1.4f, 1.4f, 1.4f), 0.2f, 10, 1f);
            });
            currentCorrectItem = null;

            if (correctChoices >= totalRightItems[CurrentLevelPart])
                StartCoroutine(WinSequence());

        }
        else
        {
            wrongChoices++;
            item.GetComponent<Image>().DOFade(1f, 0.2f).OnComplete(() =>
            {
                item.PlayWrongShake();
            });
        }

        item.DisableInteraction();
    }
    public override IEnumerator WinSequence()
    {
        gameFinished = true;
        if (CurrentLevelPart < parts.Count - 1)
        {
            yield return new WaitForSeconds(1);
            parts[CurrentLevelPart].SetActive(false);
            CurrentLevelPart++;
            parts[CurrentLevelPart].SetActive(true);
            if (part2Background != null)
                background.sprite = part2Background;
            correctChoices = 0;
            gameFinished = false;
            yield break;
        }
        Line.AnimateLine();
        yield return new WaitForSeconds(9);

        LevelManager.Instance?.IncreaseLevelOpen(currentLevelIndex);
        DisableObjects();

        HideStars();

        yield return new WaitForSeconds(0f);
        SoundManager.Instance?.PlaySound(winSoundKey);
        if (winPanel) winPanel.SetActive(true);
        int finalCorrectAnswers = correctChoices - wrongChoices;

        int starsToShow = finalCorrectAnswers >= totalRightItems[CurrentLevelPart]
            ? 3
            : finalCorrectAnswers >= totalRightItems[CurrentLevelPart] / 2
                ? 2
                : 1;

        for (int i = 0; i < starsToShow; i++)
        {
            if (winStars[i] != null)
            {
                winStars[i].SetActive(true);
                yield return StartCoroutine(PlayStarPop(winStars[i]));
            }
        }

    }

    public override IEnumerator LoseSequence()
    {
        DisableObjects();
        SoundManager.Instance?.PlaySound(loseSoundKey);
        if (losePanel) losePanel.SetActive(true);
        yield break;
    }


    public void ShowItem(GameObject item)
    {
        foreach (var itm in iconItems)
        {
            itm.SetActive(false);
        }
        item.SetActive(true);
    }

    public void IncreaseCount()
    {
        if (CurrentCount + CurrentPurchesedItems < 10)
        {
            CurrentCount++;
        }
        UpdateCountUiText();
    }

    public void DecreaseCount()
    {
        if (CurrentCount > 0)
        {
            CurrentCount--;
        }
        UpdateCountUiText();
    }

    public void SetItemToBuy(GameObject item)
    {
        itemToBuy = item;
    }

    public void SetItemPrice(int price)
    {
        currentItemPrice = price;
    }


    public void BuyItem()
    {
        if (itemToBuy != null)
        {
            CurrentPurchesedItems += CurrentCount;
            if (CurrentCount > 0)
            {
                itemToBuy.GetComponentInChildren<SelectableItem>()?.PlayCorrectAnimation();
                if (itemsPurchesed.ContainsKey(itemToBuy) == false)
                {
                    itemsPurchesed.Add(itemToBuy, CurrentCount);
                    itemsPrice.Add(itemToBuy, currentItemPrice);
                }
                else
                {
                    itemsPurchesed[itemToBuy] += CurrentCount;
                    itemsPrice[itemToBuy] = currentItemPrice;
                }
                CurrentCount = 0;
            }

            if (CurrentPurchesedItems >= 10)
            {
                FinalHolder.SetActive(true);
                FinalCount.SetActive(true);
                int counter = 0;
                foreach (var count in itemsPurchesed)
                {
                    GameObject obj = Instantiate(count.Key, FinalHolder.transform);
                    foreach (var itm in itemsPrice)
                    {
                        if (itm.Key == count.Key)
                        {
                            obj.GetComponentInChildren<TextMeshProUGUI>().text = count.Value.ToString();
                            counter += (itm.Value * count.Value);
                        }
                    }
                }
                AssignAnswers(counter);
            }
        }
        UpdateCountUiText();
    }

    public void UpdateCountUiText()
    {
        if (countUiText != null)
        {
            countUiText.text = CurrentCount.ToString();
        }
    }

    public void AssignAnswers(int correctValue)
    {
        List<int> allValues = new List<int> { correctValue };
        while (allValues.Count < 3)
        {
            int offset = Random.Range(1, 6);
            int fakeValue = Random.value < 0.5f ? correctValue + offset : correctValue - offset;
            if (!allValues.Contains(fakeValue)) allValues.Add(fakeValue);
        }

        Shuffle(allValues);

        for (int i = 0; i < answerButtons.Count; i++)
        {
            int value = allValues[i];

            TMP_Text txt = answerButtons[i].GetComponentInChildren<TMP_Text>();
            txt.text = value.ToString();

            answerButtons[i].onClick.RemoveAllListeners();

            if (value == correctValue)
            {
                answerButtons[i].onClick.AddListener(() => CorrectAnswer());
            }
            else
            {
                answerButtons[i].onClick.AddListener(() => WrongAnswer());
            }
        }
    }

    private void CorrectAnswer()
    {
        correctChoices = allItems.Count;
        StartCoroutine(WinSequence());
    }

    private void WrongAnswer()
    {
        StartCoroutine(LoseSequence());
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[rnd];
            list[rnd] = list[i];
            list[i] = temp;
        }
    }
}
