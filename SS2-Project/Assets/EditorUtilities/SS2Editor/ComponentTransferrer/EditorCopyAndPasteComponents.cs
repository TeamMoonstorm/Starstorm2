using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorCopyAndPasteComponents {

    public static List<ComponentCopierBase> ComponentCopiers = new List<ComponentCopierBase> {
        new NotRetardedUnityComponentCopier(),
        new ChildLocatorCopier(),
        new RagdollControllerCopier(),
        new UnhandledComponentsReporter(),
    };

    [MenuItem("Edit/record certain components #c")]
    public static void copyComponents() {
        string copyReport = "";

        GameObject selected = Selection.activeGameObject;

        for (int i = 0; i < ComponentCopiers.Count; i++) {

            ComponentCopiers[i].StoreComponent(selected);

            if (!string.IsNullOrEmpty(ComponentCopiers[i].copyReport)) {
                copyReport += ComponentCopiers[i].copyReport;
            }
        }

        if (string.IsNullOrEmpty(copyReport)) {
            Debug.Log("did not copy any components", selected);
        } else {
            Debug.Log($"copying from {selected.name}: {copyReport}\n", selected);
        }

    }

    [MenuItem("Edit/transfer recorded components #v")]
    public static void pasteComponents() {

        string bigLog = "";

        GameObject selected = Selection.activeGameObject;

        for (int i = 0; i < ComponentCopiers.Count; i++) {
            ComponentCopiers[i].PasteComponent(selected);
            bigLog += ComponentCopiers[i].pasteReport;
        }

        if (string.IsNullOrEmpty(bigLog)) {
            Debug.Log("but nobody came");
        } else {
            Debug.Log($"Paste Components Report for {selected}: {bigLog}\n", selected);
        }
    }
}
