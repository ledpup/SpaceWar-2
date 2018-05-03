using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public static class Targeting
    {
        public static GameObject AquireTaget(string subjectTag, Vector3 subjectPosition, List<string> factions)
        {
            if (subjectTag == "Untagged")
                return null;

            var enemies = new List<GameObject>();
            factions
                .Where(x => x != subjectTag)
                .ToList()
                .ForEach(x => enemies.AddRange(GameObject.FindGameObjectsWithTag(x)));

            enemies = enemies
                        .OrderBy(x => Vector3.Distance(x.transform.position, subjectPosition))
                        .ToList();

            var target = enemies.FirstOrDefault();
            return target;
        }
    }
}
