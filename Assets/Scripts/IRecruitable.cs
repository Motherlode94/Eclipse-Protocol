using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRecruitable
{
    bool IsRecruited { get; }
    void Recruit();  // Recruter un mercenaire
    void Dismiss();  // Annuler le recrutement
}
