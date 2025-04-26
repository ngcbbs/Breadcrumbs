namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// Enum defining all possible enemy AI states
    /// </summary>
    public enum EnemyStateType {
        Idle,
        Patrol,
        Chase,
        Attack,
        Retreat,
        MaintainDistance,
        Stunned,
        Dead,
        // Boss-specific states
        AreaAttack,
        Charge,
        Summon,
        Vulnerable
    }
}