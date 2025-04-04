using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Objects/BoardGame/Question")]
public class QuestionData : ScriptableObject
{
    public string question;
    public string correctOption;
    public List<string> incorrectOptions;
}
