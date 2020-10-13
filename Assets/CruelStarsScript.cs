using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class CruelStarsScript : MonoBehaviour
{
	public KMAudio Audio;
  public KMBombInfo Bomb;
  public KMBombModule Module;

	public AudioClip[] StarMusical;
	public KMSelectable[] StarFormation;
	public TextMesh Number;
	public KMSelectable[] ComplementaryButtons;
	public MeshRenderer[] Stars;
	public Material[] Colors;
	public Material[] OrderColors;
	public GameObject Screen;
	private List<string> idModsonBomb;

	private String[,] MainTable = new String[8,8]{
		{"1435223",	"323333",	"34323",	"2551",	"5545",	"12134435",	"212312",	"545"},
		{"5432154",	"1234555",	"12121233",	"223344",	"21",	"45132",	"11235",	"1554333"},
		{"51351",	"333",	"45",	"4334433",	"11111112",	"24123",	"45321",	"445"},
		{"2344",	"321",	"143",	"2234",	"43551",	"3251",	"34243",	"12345"},
		{"155",	"125",	"233334",	"1111",	"255",	"522",	"22",	"2"},
		{"212312",	"433234",	"2343",	"444414",	"5112",	"5233",	"24444",	"123123"},
		{"3212355",	"2341351",	"1515",	"42323",	"21151",	"12223",	"4412",	"151543"},
		{"23155",	"43211",	"11522",	"34555",	"5232",	"33445",	"44123",	"52432"}
	};
	private int CurrentR = 0,CurrentC = 0;
	private int[] ColorsOfButtons = new int[5];

	private String InputtedString = "";
	private String SolutionString = "";
	private String FinalSolutionString = "";
	private String DisplayedWord = "";
	private int[] Costing = new int[3];
	private int CostingIndex = 0;
	private int MovementMultiplier = 0;
	private bool appendOrNot = false;


	private String[] Words = new String[]{
		"ABOLISH","ABSORB","ACCUSE","ADJUST","ANALYSE","ANTICIPATE","BOIL","BURY","CEASE","CLASSIFY","DEFEAT","DESCEND","DETECT","DIMINISH",
		"ELIMINATE","ESCAPE","EXCHANGE","EXPAND","EXPLOIT","INHIBIT","INSTALL","JACKED","OBTAIN","QUALIFY","REGRET","RENDER","SCATTER","SCREAM","SUPPLY",
		"UNDERGO","UPDATE"
	};


	bool Animating = false;

	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		idModsonBomb = Bomb.GetModuleIDs();
		for (int a = 0; a < 5; a++)
		{
			int Starlight = a;
			StarFormation[Starlight].OnInteract += delegate{StarPress(Starlight);return false;};
		}

		for (int b = 0; b < 3; b++)
		{
			int Complements = b;
			ComplementaryButtons[Complements].OnInteract += delegate{Status(Complements);return false;};
		}
	}

	void Start()
	{
		GenerateHopping();
		CalculateInitialPosition();
		TableMovements();
		StringModifying();
		Unicorn();
	}

	void GenerateHopping()
	{
		List<int> digits = new List<int>(new int[] {0,1,2,3,4,5,6,7,8,9});
		digits.Shuffle();
		for(int i=0;i<3;i++)
		{
			Costing[i] = digits[i];
		}
		Debug.LogFormat("[Cruel Stars #{0}] The number shown in the module is: {1}, {2}, {3}", moduleId, Costing[0], Costing[1], Costing[2]);
		Number.text = Costing[CostingIndex].ToString();

		DisplayedWord = Words[UnityEngine.Random.Range(0,Words.Length)];
		Screen.GetComponent<TextMesh>().text = DisplayedWord;
		Debug.LogFormat("[Cruel Stars #{0}] The word shown in the module is: {1}", moduleId, Screen.GetComponent<TextMesh>().text);

		for(int i=0;i<StarFormation.Length;i++)
		{
			int pickedColor = UnityEngine.Random.Range(0,OrderColors.Length);
			ColorsOfButtons[i] = pickedColor;
			StarFormation[i].GetComponent<MeshRenderer>().material = OrderColors[pickedColor];
		}

		if(Costing[0]%2+Costing[1]%2+Costing[2]%2 < 2)
		{
			appendOrNot = true;
		}
		if(appendOrNot){Debug.LogFormat("[Cruel Stars #{0}] You must append your string.", moduleId);}
		else{Debug.LogFormat("[Cruel Stars #{0}] You must prepend your string.", moduleId);}
	}

	void CalculateInitialPosition()
	{
		var SNarr = Bomb.GetSerialNumber();

		for(int i=0;i<6;i++)
		{
			if(SNarr[i]>='A'&&SNarr[i]<='Z'){
				CurrentR = SNarr[i];
				break;
			}
		}
		for(int i=5;i>=0;i--)
		{
			if(SNarr[i]>='A'&&SNarr[i]<='Z'){
				CurrentC = SNarr[i];
				break;
			}
		}
		for(int i=0;i<6;i++)
		{
			if(SNarr[i]>='0'&&SNarr[i]<='9'){
				if(MovementMultiplier==0)
				{
					MovementMultiplier = -1;
				}
				else if(MovementMultiplier==-1)
				{
					MovementMultiplier = SNarr[i]-'0';
					break;
				}
			}
		}
		if(MovementMultiplier==0)
		{
			MovementMultiplier = 10;
		}

		//if(SNf>='0'&&SNf<='9'){SNf-='0';}else{SNf-='A'-10;}
		//if(SNl>='0'&&SNl<='9'){SNl-='0';}else{SNl-='A'-10;}
		CurrentR-='A'-10;CurrentC-='A'-10;CurrentR%=8;CurrentC%=8;

		Debug.LogFormat("[Cruel Stars #{0}] Your initial position is ({1},{2}).", moduleId, CurrentR, CurrentC);
	}

	void TableMovements()
	{
		Debug.LogFormat("[Cruel Stars #{0}] All movements are multiplied by {1}.", moduleId, MovementMultiplier);
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);

		if(Vowel(DisplayedWord[DisplayedWord.Length-1]))
		{
			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
		}
		else
		{
			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

		if(DisplayedWord.Length == 6)
		{
			CurrentR = 7-CurrentR;
			CurrentC = 7-CurrentC;
		}
		else
		{
			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

		if(DisplayedWord[0]>='I'&&DisplayedWord[0]<='U')
		{
			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
		}
		else
		{
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

		if(DisplayedWord.Contains('T'))
		{
			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
		}
		else
		{
			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

		if(DisplayedWord.Length == 4)
		{
			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR = 7-CurrentR;
			CurrentC = 7-CurrentC;
		}
		else
		{
			var SNarr = Bomb.GetSerialNumber();
			if(SNarr[5]%2==0)
			{
				for(int i=0;i<MovementMultiplier;i++)
				{
					int container = CurrentC;
					CurrentC = 7-CurrentR;
					CurrentR = container;
				}
			}
			else
			{
				for(int i=0;i<MovementMultiplier;i++)
				{
					int container = CurrentR;
					CurrentR = 7-CurrentC;
					CurrentC = container;
				}
			}
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

		if(DisplayedWord.Contains('I'))
		{
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
		}
		else
		{
			CurrentR = 7-CurrentR;
			CurrentC = 7-CurrentC;
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);


		bool repeatedletter = false;
		for(int i=0;i<DisplayedWord.Length-1;i++)
		{
			if(DisplayedWord[i] == DisplayedWord[i+1])
			{
				repeatedletter = true;
			}
		}

		if(repeatedletter)
		{
			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
		}
		else
		{
			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
		}
		SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
		Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

		if(DisplayedWord == "SCREAM")
		{
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}).", moduleId, CurrentR, CurrentC);

			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}).", moduleId, CurrentR, CurrentC);

			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentR -= MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}).", moduleId, CurrentR, CurrentC);

			CurrentC += MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);

			CurrentC -= MovementMultiplier;
			CurrentC = Modulo(CurrentC,8);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}).", moduleId, CurrentR, CurrentC);

			CurrentR += MovementMultiplier;
			CurrentR = Modulo(CurrentR,8);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}).", moduleId, CurrentR, CurrentC);

			CurrentR = 7-CurrentR;
			CurrentC = 7-CurrentC;
			SolutionString = AddString(SolutionString,MainTable[CurrentR,CurrentC]);
			Debug.LogFormat("[Cruel Stars #{0}] Moved to ({1},{2}). {3} were added.", moduleId, CurrentR, CurrentC, MainTable[CurrentR,CurrentC]);
		}






		Debug.LogFormat("[Cruel Stars #{0}] Solution string before modifying is {1}.", moduleId, SolutionString);
	}

	int Modulo(int N, int X)
	{
		while(N>=X)
		{
			N-=X;
		}
		while(N<0)
		{
			N+=X;
		}
		return N;
	}

	bool Vowel(char N)
	{
		if(N=='A'||N=='E'||N=='I'||N=='O'||N=='U')
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	String AddString(String Base, String Addition)
	{
		if(appendOrNot){return Base+Addition;}
		else{return Addition+Base;}
	}





	void StringModifying()
	{
		while(SolutionString.Length%3!=0)
		{
			SolutionString = SolutionString+"1";
		}

		int iniR = ColorsOfButtons[0]/4,iniG = (ColorsOfButtons[0]%4)/2,iniB = ColorsOfButtons[0]%2;
		for(int i=1;i<5;i++)
		{
			iniR+= ColorsOfButtons[i]/4;
			iniG+= (ColorsOfButtons[i]%4)/2;
			iniB+= ColorsOfButtons[i]%2;
			iniR%=2;iniG%=2;iniB%=2;
		}
		int finalColor = iniR*4+iniG*2+iniB;

		for(int i=0;i<SolutionString.Length/3;i++)
		{
			//int finalColor = (ColorsOfButtons[SolutionString[3*i]-'1']/4)*4+((ColorsOfButtons[SolutionString[3*i+1]-'1']%4)/2)*2+ColorsOfButtons[SolutionString[3*i+2]-'1']%2;

			if(finalColor == 1)
			{
				FinalSolutionString += (SolutionString[3*i]).ToString()+SolutionString[3*i+1].ToString()+((SolutionString[3*i]-'0'+SolutionString[3*i+1]-'0'+SolutionString[3*i+2]-'0')%5+1).ToString();
			}
			else if(finalColor == 2)
			{
				FinalSolutionString += (SolutionString[3*i]).ToString()+((SolutionString[3*i]-'0'+SolutionString[3*i+1]-'0'+SolutionString[3*i+2]-'0')%5+1).ToString()+(SolutionString[3*i+2]).ToString();
			}
			else if(finalColor == 4)
			{
				FinalSolutionString += ((SolutionString[3*i]-'0'+SolutionString[3*i+1]-'0'+SolutionString[3*i+2]-'0')%5+1).ToString()+(SolutionString[3*i+1]).ToString()+(SolutionString[3*i+2]).ToString();
			}

			else if(finalColor == 3)
			{
				FinalSolutionString += SolutionString[3*i].ToString()+SolutionString[3*i+2].ToString()+SolutionString[3*i+1].ToString();
			}
			else if(finalColor == 5)
			{
				FinalSolutionString += SolutionString[3*i+2].ToString()+SolutionString[3*i+1].ToString()+SolutionString[3*i].ToString();
			}
			else if(finalColor == 6)
			{
				FinalSolutionString += SolutionString[3*i+1].ToString()+SolutionString[3*i].ToString()+SolutionString[3*i+2].ToString();
			}

			else
			{
				FinalSolutionString += SolutionString[3*i].ToString()+SolutionString[3*i+1].ToString()+SolutionString[3*i+2].ToString();
			}

		}

		Debug.LogFormat("[Cruel Stars #{0}] Solution string after modifying is {1}.", moduleId, FinalSolutionString);

	}









	void Unicorn()
	{
		if(idModsonBomb.Contains("stars") && Bomb.IsIndicatorOn("BOB") && Bomb.GetSerialNumberLetters().Any(x => x == 'L' && x == 'W'))
		{
			FinalSolutionString = "115111541151115411511154543543211151115411511154115111544545451";
		}
	}




	void StarPress(int Starlight)
	{
		StarFormation[Starlight].AddInteractionPunch(0.2f);
		if (!(Animating||ModuleSolved))
		{
			Audio.PlaySoundAtTransform(StarMusical[Starlight].name, transform);
			InputtedString += (Starlight+1).ToString();
		}
	}


	void Status(int Complements)
	{
		ComplementaryButtons[Complements].AddInteractionPunch(0.2f);
		if (!(Animating||ModuleSolved))
		{
			if (Complements == 0)
			{
				Audio.PlaySoundAtTransform(StarMusical[5].name, transform);
				InputtedString = "";
				Debug.LogFormat("[Cruel Stars #{0}] All inputs has been cleared", moduleId);
			}

			else if (Complements == 1)
			{
				StartCoroutine(Starlighting());
				Animating = true;
				if (InputtedString=="")
				{
					Debug.LogFormat("[Cruel Stars #{0}] The sequence you submitted is: Literally Nothing!", moduleId);
				}
				else
				{
					Debug.LogFormat("[Cruel Stars #{0}] The sequence you submitted is: {1}", moduleId, InputtedString);
				}
			}

			else if (Complements == 2)
			{
				CostingIndex = (CostingIndex+1)%3;
				Number.text = Costing[CostingIndex].ToString();
				Audio.PlaySoundAtTransform(StarMusical[8].name, transform);
			}
		}
	}

	IEnumerator Starlighting()
	{
		bool Mistake = false;
		if (InputtedString!=FinalSolutionString)
		{
			Mistake = true;
		}
		Audio.PlaySoundAtTransform(StarMusical[6].name, transform);


        //Yellow flashes
				Stars[0].material = Colors[2];
				yield return new WaitForSecondsRealtime(1.05f);
				Stars[1].material = Colors[2];
				yield return new WaitForSecondsRealtime(1.05f);
				Stars[2].material = Colors[2];
				yield return new WaitForSecondsRealtime(1.05f);
				Stars[3].material = Colors[2];
				yield return new WaitForSecondsRealtime(1.05f);
				Stars[4].material = Colors[2];
        yield return new WaitForSecondsRealtime(1.05f);

        for (int x = 0; x < Stars.Count(); x++)
        {
            Stars[x].material = Colors[4];
        }
        yield return new WaitForSecondsRealtime(1f);
        for (int a = 0; a < Stars.Count(); a++)
        {
            Stars[a].material = Colors[4];
        }

        //Fast red flashes
        Stars[0].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[1].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[2].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[3].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[4].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);

        //Fast white flashes
        Stars[0].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[1].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[2].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[3].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[4].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);

        //Fast red flashes
        Stars[0].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[1].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[2].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[3].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[4].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);

        //Fast white flashes
        Stars[0].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[1].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[2].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[3].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[4].material = Colors[4];
        yield return new WaitForSecondsRealtime(0.05f);

        //Fast red flashes
        Stars[0].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[1].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[2].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[3].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);
        Stars[4].material = Colors[0];
        yield return new WaitForSecondsRealtime(0.05f);


        yield return new WaitForSecondsRealtime(0.03f);
        for (int x = 0; x < Stars.Count(); x++)
        {
            Stars[x].material = Colors[4];
        }
        yield return new WaitForSecondsRealtime(0.03f);
        for (int a = 0; a < Stars.Count(); a++)
        {
            Stars[a].material = Colors[0];
        }

        yield return new WaitForSecondsRealtime(0.03f);
        for (int x = 0; x < Stars.Count(); x++)
        {
            Stars[x].material = Colors[4];
        }
        yield return new WaitForSecondsRealtime(0.03f);
        for (int a = 0; a < Stars.Count(); a++)
        {
            Stars[a].material = Colors[0];
        }

        yield return new WaitForSecondsRealtime(0.03f);
        for (int x = 0; x < Stars.Count(); x++)
        {
            Stars[x].material = Colors[4];
        }
        yield return new WaitForSecondsRealtime(0.03f);
        for (int a = 0; a < Stars.Count(); a++)
        {
            Stars[a].material = Colors[0];
        }

        yield return new WaitForSecondsRealtime(0.03f);
        for (int x = 0; x < Stars.Count(); x++)
        {
            Stars[x].material = Colors[4];
        }




        yield return new WaitForSecondsRealtime(0.65f);
		Stars[0].material = Colors[3];
		yield return new WaitForSecondsRealtime(1.04f);
		Stars[1].material = Colors[3];
		yield return new WaitForSecondsRealtime(1.04f);
		Stars[2].material = Colors[3];
		yield return new WaitForSecondsRealtime(1.04f);
		Stars[3].material = Colors[3];
		yield return new WaitForSecondsRealtime(1.04f);
		Stars[4].material = Colors[3];
		yield return new WaitForSecondsRealtime(1.04f);
		for (int x = 0; x < Stars.Count(); x++)
		{
			Stars[x].material = Colors[4];
		}
		yield return new WaitForSecondsRealtime(0.90f);
		for (int a = 0; a < Stars.Count(); a++)
		{
			Stars[a].material = Colors[3];
		}
		yield return new WaitForSecondsRealtime(1.7f);

		if (Mistake == true)
		{
			Debug.LogFormat("[Cruel Stars #{0}] The sequence given is incorrect. A strike was given.", moduleId);
			Module.HandleStrike();

			InputtedString="";
			for (int x = 0; x < Stars.Count(); x++)
			{
				Stars[x].material = Colors[0];
			}
			yield return new WaitForSecondsRealtime(0.6f);
			for (int x = 0; x < Stars.Count(); x++)
			{
				Stars[x].material = Colors[3];
			}

			for(int i=0;i<5;i++)
			{
				Stars[i].material = OrderColors[ColorsOfButtons[i]];
			}
		}

		else
		{
			Debug.LogFormat("[Cruel Stars #{0}] The sequence given is correct. Module solved.", moduleId);
			Module.HandlePass();
			Audio.PlaySoundAtTransform(StarMusical[7].name, transform);
			ModuleSolved = true;
			Number.text = "";
			for (int x = 0; x < Stars.Count(); x++)
			{
				Stars[x].material = Colors[1];
				yield return new WaitForSecondsRealtime(0.2f);
			}
		}

		Animating = false;
	}

	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press the buttons that is in a star formation, use the command !{0} press [1-5] (You can perform the command in a chain) | To toggle the digit on center, use the command !{0} toggle | To submit your answer, use the command !{0} submit | To clear all your inputs, use the command !{0} clear";
    #pragma warning restore 414
	string[] Flashlight = {"1", "2", "3", "4", "5"};

	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			/*if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length.";
				yield break;
			}*/

			if (Animating == true)
			{
				yield return "sendtochaterror The module is performing an animation. Command ignored";
				yield break;
			}

			for(int i=1;i<parameters.Length;i++)
			{
				foreach (char c in parameters[i])
				{
					if (!c.ToString().EqualsAny(Flashlight))
					{
						yield return "sendtochaterror The current character is not between 1-5.";
						yield break;
					}
				}
			}

			for(int i=1;i<parameters.Length;i++)
			{
				foreach (char c in parameters[i])
				{
					StarFormation[Int32.Parse(c.ToString())-1].OnInteract();
					yield return new WaitForSecondsRealtime(0.05f);
				}
			}
		}

		if (Regex.IsMatch(command, @"^\s*toggle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (Animating == true)
			{
				yield return "sendtochaterror The module is performing an animation. Command ignored";
				yield break;
			}
			ComplementaryButtons[2].OnInteract();
		}

		if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (Animating == true)
			{
				yield return "sendtochaterror The module is performing an animation. Command ignored";
				yield break;
			}
			ComplementaryButtons[0].OnInteract();
		}

		if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (Animating == true)
			{
				yield return "sendtochaterror The module is performing an animation. Command ignored";
				yield break;
			}
			yield return "solve";
			yield return "strike";
			ComplementaryButtons[1].OnInteract();
		}
	}
}
