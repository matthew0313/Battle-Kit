using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIElement : MonoBehaviour
{
    [SerializeField] Text m_skillName;
    [SerializeField] Transform m_skillBar;
    public Text skillName { get { return m_skillName; } }
    public Transform skillBar { get { return m_skillBar; } }
}
