namespace Breadcrumbs.AISystem {
    public static class AIContextDataExtensions {
        public static void UpdateCombatData(this AIContextData context) {
            if (context.customData.ContainsKey("inMeleeRange")) {
                bool inRange = context.distanceToTarget <=
                               (context.customData.TryGetValue("meleeAttackRange", out var value) ? (float)value : 2f);
                context.customData["inMeleeRange"] = inRange;
            }

            if (context.customData.ContainsKey("inRangedRange")) {
                bool inRange = context.distanceToTarget <=
                               (context.customData.TryGetValue("rangedAttackRange", out var value) ? (float)value : 10f);
                context.customData["inRangedRange"] = inRange;
            }
        }
    }
}