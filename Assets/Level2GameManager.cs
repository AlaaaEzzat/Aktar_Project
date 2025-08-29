using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level2GameManager : GameManager
{
    public override void OnItemSelected(SelectableItem item)
    {
        if (gameFinished) return;
        if (item.isCorrect)
        {
            correctChoices++;
            item.PlayCorrectAnimation();
            ApplyImageEffect(item);
            currentCorrectItem = null;

            savingPoints += item.pointsType == Points.Saving ? 10 : 0;

            if (correctChoices >= totalRightItems[CurrentLevelPart])
                StartCoroutine(WinSequence());

        }
        else
        {
            wrongChoices++;
            item.PlayWrongShake();

            if (item.pointsType == Points.Spending)
                spendingPoints += 10;
            else if (item.pointsType == Points.Keeping)
                keepingPoints += 10;
        }

        item.DisableInteraction();
        UpdateScoreTexts();
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
        yield return new WaitForSeconds(1);

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
}
