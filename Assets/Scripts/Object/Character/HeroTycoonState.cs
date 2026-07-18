using UnityEngine;

public enum TycoonState
{
    Idle,
    Walking,
    Rest,
    Gym
}

public interface IHeroTycoonState
{
    void Enter(HeroMovingAgent agent);
    void Update(HeroMovingAgent agent);
    void Exit(HeroMovingAgent agent);
}

public class HeroTycoonState_Idle : IHeroTycoonState
{
    public void Enter(HeroMovingAgent agent)
    {
        Animator animator = agent.GetAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("Idle", true);
        animator.SetBool("Walking", false);
    }

    public void Exit(HeroMovingAgent agent)
    {
        Animator animator = agent.GetAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("Idle", false);
    }

    public void Update(HeroMovingAgent agent)
    {

    }
}

public class HeroTycoonState_Walking : IHeroTycoonState
{
    public void Enter(HeroMovingAgent agent)
    {
        Animator animator = agent.GetAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("Idle", false);
        animator.SetBool("Walking", true);
    }

    public void Exit(HeroMovingAgent agent)
    {
        Animator animator = agent.GetAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("Walking", false);
    }

    public void Update(HeroMovingAgent agent)
    {

    }
}

public class HeroTycoonState_Rest : IHeroTycoonState
{
    public void Enter(HeroMovingAgent agent)
    {
        Animator animator = agent.GetAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger("Rest");
    }

    public void Exit(HeroMovingAgent agent)
    {

    }

    public void Update(HeroMovingAgent agent)
    {

    }
}

public class HeroTycoonState_Gym : IHeroTycoonState
{
    public void Enter(HeroMovingAgent agent)
    {
        Animator animator = agent.GetAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger("Gym");
    }

    public void Exit(HeroMovingAgent agent)
    {

    }

    public void Update(HeroMovingAgent agent)
    {

    }
}