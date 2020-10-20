using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System;

class WebGLBuilder
{
	static void build()
	{
		string[] scenes = {
			"Assets/Scenes/GameMain.unity",
			"Assets/Scenes/Lobby.unity",
			"Assets/Scenes/Gacha.unity",
			"Assets/Scenes/GachaMenu.unity",
			"Assets/Scenes/ChracterList.unity",
			"Assets/Scenes/Index.unity",
			"Assets/Scenes/CharacterSelect.unity"
		};

		UnityEditor.BuildPipeline.BuildPlayer(scenes, "WebGL-Dist", UnityEditor.BuildTarget.WebGL, UnityEditor.BuildOptions.Development);
	}
}