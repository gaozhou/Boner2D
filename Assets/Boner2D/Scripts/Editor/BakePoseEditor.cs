/*
The MIT License (MIT)

Copyright (c) 2014 - 2019 Play-Em

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

namespace Boner2D {
	[ExecuteInEditMode]
	public class BakePoseEditor : EditorWindow {

		private GameObject animatorObject;
		private Dictionary<Skeleton, Pose> Poses = new Dictionary<Skeleton, Pose>();

		[MenuItem("Boner2D/Bake Poses")]
		protected static void ShowBakePoseEditor() {
			var wnd = GetWindow<BakePoseEditor>();
			wnd.minSize = new Vector2(285, 325);
			wnd.titleContent.text = "Bake Poses";
			wnd.Show();
		}

		public void OnGUI() {
			GUILayout.Label("Bake Poses", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("GameObject with Skeletons to Bake Pose", EditorStyles.boldLabel);
			animatorObject = (GameObject)EditorGUILayout.ObjectField(animatorObject, typeof(GameObject), true);
			if (animatorObject != null) {

				EditorGUILayout.Separator();

				GUILayout.Label("Bake the Poses into selected Keyframe", EditorStyles.boldLabel);

				if (GUILayout.Button("Bake Poses")) {
					BakePose();
				}

				if (Poses.Count > 0) {
					EditorGUILayout.Separator();

					EditorGUILayout.Separator();
					if (GUILayout.Button("Clear Poses")) {
						Poses = new Dictionary<Skeleton, Pose>();
					}

					EditorGUILayout.Separator();

					GUILayout.Label("Disable IK before Applying Poses", EditorStyles.boldLabel);

					EditorGUILayout.Separator();

					if (GUILayout.Button("Disable Skeletons IK")) {
						DisableSkeletonsIK();
					}

					EditorGUILayout.Separator();

					if (GUILayout.Button("Enable Skeletons IK")) {
						EnableSkeletonsIK();
					}

					EditorGUILayout.Separator();

					if (GUILayout.Button("Apply Poses")) {
						ApplyPoses();
					}

					EditorGUILayout.Separator();

					GUILayout.Label("Only Saves Active GameObjects in Poses", EditorStyles.boldLabel);
					if (GUILayout.Button("Save Poses")) {
						SavePoses();
					}
				}


			} 
			else {
				EditorGUILayout.HelpBox("Please select a GameObject with Skeletons to Bake Pose and have Animation window open and set to Record.", MessageType.Error);

				EditorApplication.ExecuteMenuItem("Window/Animation");
			}
		}

		void BakePose () {
			if (animatorObject != null) {
				Skeleton[] skeletons = animatorObject.transform.root.gameObject.GetComponentsInChildren<Skeleton>(true);

				Poses = new Dictionary<Skeleton, Pose>();

				foreach (Skeleton s in skeletons) {
					if (s.gameObject.activeInHierarchy) {
						Pose bakedPose = s.CreatePose(false);
						Poses.Add(s, bakedPose);

						Debug.Log("Added Baked Pose for " + s.name);
					}

					UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				}

				Debug.Log("Finished baking poses for " + animatorObject.gameObject.name);
			}
		}

		private void DisableSkeletonsIK(){
			foreach (Skeleton s in Poses.Keys) {
				Undo.RecordObject(s, "Disable IK");

				s.IK_Enabled = false;

				EditorUtility.SetDirty (s);

				Debug.Log("Disabled IK for " + s.name);
			}
		}

		private void EnableSkeletonsIK(){
			foreach (Skeleton s in Poses.Keys) {
				Undo.RecordObject(s, "Disable IK");

				s.IK_Enabled = true;

				EditorUtility.SetDirty (s);

				Debug.Log("Enabled IK for " + s.name);
			}
		}

		private void ApplyPoses(){
			foreach (Skeleton s in Poses.Keys) {
				bool isIK = s.IK_Enabled; 
				s.IK_Enabled = false;

				Undo.RecordObject(s, "Apply Pose");

				s.RestorePose((Pose)Poses[s]);

				EditorUtility.SetDirty (s);

				s.IK_Enabled = isIK;

				Debug.Log("Applied Baked Poses for " + s.name);
			}
		}

		private void SavePoses(){
			if (!Directory.Exists("Assets/Poses")) {
				AssetDatabase.CreateFolder("Assets", "Poses");
				AssetDatabase.Refresh();
			}

			foreach (Skeleton s in Poses.Keys) {
				if ((Pose)Poses[s] == null) {
					Debug.LogError("No Pose exists for " + s.name);
				}
				else {
					ScriptableObjectUtility.CreateAsset((Pose)Poses[s], "Poses/" + s.name + " Pose");
				}

				Debug.Log("Saved Baked Poses for " + s.name);
			}
		}
	}
}