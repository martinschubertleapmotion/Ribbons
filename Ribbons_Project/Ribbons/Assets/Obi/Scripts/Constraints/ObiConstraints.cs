using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

/**
 * Class to hold per-actor information for a kind of constraints.
 *
 * You can only add or remove constraints when the actor is not in the solver. If you need to continously
 * add and remove constraints, the best approach is to reserve a bunch of constraints beforehand and then
 * individually activate/deactivate/update them.
 */
public abstract class ObiConstraints : MonoBehaviour, IObiSolverClient 
{	

	[NonSerialized] protected ObiActor actor;
	[NonSerialized] protected int indicesOffset;
	[NonSerialized] protected bool inSolver;

	[HideInInspector] public List<bool> activeStatus = new List<bool>();		/**< activation flag for each constraint.*/

	public ObiActor Actor{
		get{return actor;}
	}

	public bool InSolver{
		get{return inSolver;}
	}

	public abstract void Initialize();

	/**
	 * Returns a list of all constraint indices involving at least one the provided particle indices.
	 */
	public abstract List<int> GetConstraintsInvolvingParticle(int particleIndex);

	protected abstract void OnAddToSolver(object info);
	protected abstract void OnRemoveFromSolver(object info);

	protected virtual void OnAddedToSolver(object info){}
	protected virtual void OnRemovedFromSolver(object info){}

	public virtual void PushDataToSolver(ObiSolverData data){}
	public virtual void PullDataFromSolver(ObiSolverData data){}

	public bool AddToSolver(object info){

		if (inSolver || actor == null || !actor.InSolver)
			return false;

		// custom addition code:
		OnAddToSolver(info);

		// update constraints if enabled, deactivate all constraints otherwise.
		if (enabled)
			UpdateConstraintActiveStatus();
		else
			DeactivateConstraints();
			
		// commit constraint changes to solver:
		actor.solver.CommitConstraints(this.GetType());

		inSolver = true;

		OnAddedToSolver(info);
		
		return true;

	}
	public bool RemoveFromSolver(object info){

		if (!inSolver || actor == null || !actor.InSolver)
			return false;

		// deactivate all constraints first:
		DeactivateConstraints();

		// custom removal code:
		OnRemoveFromSolver(info);

		// commit constraint changes to solver:
		actor.solver.CommitConstraints(this.GetType());

		inSolver = false;

		OnRemovedFromSolver(info);

		return true;

	}

	/**
	 * Deactivates all constraints in the solver, regardless of each individual constraint's state.
	 */
	protected void DeactivateConstraints(){
	
		if (actor == null || !actor.InSolver)
			return;
		
		Type type = this.GetType();
		
		// deactivate all constraints:
		for (int i = 0; i < activeStatus.Count; i++)
			actor.solver.RemoveActiveConstraint(type,indicesOffset+i);
		
		actor.solver.UpdateActiveConstraints(type);
		
	}
	
	/**
	 * Updates the activation status of all (active/inactive) constraints in the solver. Active constraints will
	 * only be marked as active in the solver if this component is enabled, they will be deactivated otherwise.
	 */
	public void UpdateConstraintActiveStatus(){
	
		if (actor == null || !actor.InSolver)
			return;
		
		Type type = this.GetType();
		
		// only activate constraints marked as active.
		for (int i = 0; i < activeStatus.Count; i++){
			if (activeStatus[i] && enabled)
				actor.solver.AddActiveConstraint(type,indicesOffset+i);
			else
				actor.solver.RemoveActiveConstraint(type,indicesOffset+i);
		}
		
		actor.solver.UpdateActiveConstraints(type);
		
	}
	
	/**
	 * Changes the offset of the constraints in the solver arrays, taking care
	 * of generating new active indices as needed. Use when the index has changed due
	 * to removal of constraints with lower indices.
	 */
	public void UpdateIndicesOffset(int newOffset){
		
		if (actor == null || !actor.InSolver)
			return;
			
		Type type = this.GetType();
		
		// only activate constraints marked as active.
		for (int i = 0; i < activeStatus.Count; i++){
		
			// remove old index
			actor.solver.RemoveActiveConstraint(type,indicesOffset+i);
			
			// add new index, if active.
			if (activeStatus[i] && enabled)
				actor.solver.AddActiveConstraint(type,newOffset+i);
				
        }
        
        // update indices offset.
		indicesOffset = newOffset;
                
		actor.solver.UpdateActiveConstraints(type);
		
	}
	
	/**
	 * When enabling constraints, active constraints get activated in the solver.
	 */
	public void OnEnable(){
		
		// get the actor in this gameobject.
		actor = GetComponent<ObiActor>();
		
		UpdateConstraintActiveStatus();
		
	}
	
	/**
 	 * When disabling constraints, all individual constraints get deactivated in the solver.
 	 */
	public void OnDisable(){

		DeactivateConstraints();
		
	}
	
	public void OnDestroy(){
		RemoveFromSolver(null);
	}
}
}

