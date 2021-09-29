using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestTextBoxScript : MonoBehaviour
{
    private int id = 40922167;
    private string dbIP = "localhost";//"129.120.215.196";

    private TextMesh boldMesh;
    private TextMesh regularMesh;

    private void Start()
    {
        boldMesh = transform.Find("BoldText").GetComponent<TextMesh>();
        regularMesh = transform.Find("RegularText").GetComponent<TextMesh>();

        StartCoroutine(LoadIDStuff());
    }

    private IEnumerator LoadIDStuff()
    {
        string loadURL = "http://" + dbIP + "/ioan_newidtester_fig_2.php?id=" + id;

        WWW loadID = new WWW(loadURL);
        yield return (loadID);

        string graphURL = "";
        List<string> oldSplitVals = loadID.text.Split(new string[] { "<br/>" }, System.StringSplitOptions.None).ToList();
        List<string> splitVals = new List<string>();

        foreach (string val in oldSplitVals)
        {
            if (val.Length < 2) splitVals.Add(val);
            else if (val.Substring(0, 6) == "http:/" || val.Substring(0, 6) == "https:") graphURL = val;
            else if (val.Substring(0, 6) == "fields" || val.Substring(0, 6) == "fleids") { /* splitVals.Remove (val); */ }
            else if (val.Substring(0, 6) == "Figure") { /* splitVals.Remove (val); */ }
            else splitVals.Add(val);
        }

        //Sanitizing
        for (int i = 0; i < splitVals.Count(); i++)
        {
            if (splitVals[i].Contains("\n")) splitVals[i] = splitVals[i].Replace("\n", "");
        }

        splitVals.RemoveAll((s) => s == "");

        //Output
        ModOutputString(splitVals, 2);
    }

    private void ModOutputString(List<string> data, int param)
    {
        switch(param)
        {
            case 0:
                string str = string.Join("\n", data.ToArray());
                GetComponent<TextMesh>().text = str;
                break;
            case 1:
                OutputOne(data);
                break;
            case 2:
                OutputTwo(data);
                break;
        }
    }

    private void OutputOne(List<string> data)
    {
        string boldText = "";
        string regularText = "";
        int maxStringSize = 0;
        List<string> sanData = new List<string>();

        foreach (string s in data)
        {
            string str = s.TrimEnd(new char[] { ' ' });
            str = new string(str.Where((c) => !char.IsControl(c)).ToArray());

            if (str.Contains(" "))
            {
                string[] splitStr = str.Split(new char[] { ' ' });

                int size = splitStr[0].Length;

                if (size > maxStringSize) maxStringSize = size;
            }

            sanData.Add(str);
        }

        foreach (string s in sanData)
        {
            string str = s;

            if (!s.Contains(' '))
            {
                boldText += "\n";
                regularText += "\n";
            }

            string[] splitStr = str.Split(new char[] { ' ' });
            int size = splitStr[0].Length;

            for (int i = 0; i < maxStringSize - size; i++)
            {
                splitStr[0] = " " + splitStr[0];
            }

            for (int i = 0; i < splitStr[0].Length; i++)
            {
                boldText += splitStr[0].ElementAt(i);
                regularText += " ";
            }

            boldText += " ";
            regularText += " ";

            if (splitStr.Length == 1)
            {
                boldText += "\n";
                regularText += "\n";
                continue;
            }

            for (int i = 0; i < splitStr[1].Length; i++)
            {
                boldText += " ";
                regularText += splitStr[1].ElementAt(i);
            }

            boldText += "\n";
            regularText += "\n";
        }

        boldMesh.fontStyle = FontStyle.Bold;
        boldMesh.alignment = TextAlignment.Left;
        boldMesh.text = boldText;

        regularMesh.fontStyle = FontStyle.Normal;
        regularMesh.alignment = TextAlignment.Left;
        regularMesh.text = regularText;
    }

    private void OutputTwo(List<string> data)
    {
        string boldText = "";
        string regularText = "";
        int maxStringSize = 0;
        List<string> sanData = new List<string>();

        foreach (string s in data)
        {
            string str = s.TrimEnd(new char[] { ' ' });
            str = new string(str.Where((c) => !char.IsControl(c)).ToArray());

            if (str.Contains(" "))
            {
                string[] splitStr = str.Split(new char[] { ' ' });

                int sizeOne = splitStr[0].Length;
                int sizeTwo = splitStr[1].Length;

                if (sizeOne > maxStringSize) maxStringSize = sizeOne;
                if (sizeTwo > maxStringSize) maxStringSize = sizeTwo;
            }

            sanData.Add(str);
        }

        foreach (string s in sanData)
        {
            string str = s;

            if (!s.Contains(' '))
            {
                boldText += "\n";
                regularText += "\n";
            }

            string[] splitStr = str.Split(new char[] { ' ' });
            int sizeOne = splitStr[0].Length;

            for (int i = 0; i < maxStringSize - sizeOne; i++)
            {
                splitStr[0] = " " + splitStr[0];
            }

            for (int i = 0; i < splitStr[0].Length; i++)
            {
                boldText += splitStr[0].ElementAt(i);
                regularText += " ";
            }

            boldText += " ";
            regularText += " ";

            if (splitStr.Length == 1)
            {
                boldText += "\n";
                regularText += "\n";
                continue;
            }

            int sizeTwo = splitStr[1].Length;

            for (int i = 0; i < maxStringSize - sizeTwo; i++)
            {
                splitStr[1] += " ";
            }

            for (int i = 0; i < splitStr[1].Length; i++)
            {
                boldText += " ";
                regularText += splitStr[1].ElementAt(i);
            }

            boldText += "\n";
            regularText += "\n";
        }

        boldMesh.fontStyle = FontStyle.Bold;
        boldMesh.alignment = TextAlignment.Left;
        boldMesh.text = boldText;

        regularMesh.fontStyle = FontStyle.Normal;
        regularMesh.alignment = TextAlignment.Left;
        regularMesh.text = regularText;
    }
}
