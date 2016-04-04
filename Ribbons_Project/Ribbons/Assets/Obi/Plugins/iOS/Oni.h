/*
 *  Oni.h
 *  Oni
 *
 *  Created by José María Méndez González on 21/9/15.
 *  Copyright (c) 2015 ArK. All rights reserved.
 *
 */

#ifndef Oni_
#define Oni_

#include "Solver.h"

namespace Oni
{
    
    struct ConstraintGroupParameters;
    class Collider;
    class Rigidbody;
    struct SphereShape;
    struct BoxShape;
    struct CapsuleShape;
    struct HeightmapShape;
    class HeightmapData;
    struct CollisionMaterial;

    extern "C"
    {
        
         ColliderGroup* CreateColliderGroup();
         void DestroyColliderGroup(ColliderGroup* group);
        
         void SetColliders(ColliderGroup* group, const Collider* colliders, const int num);
         void SetRigidbodies(ColliderGroup* group, Rigidbody* rigidbodies, const int num);
         void SetSphereShapes(ColliderGroup* group, const SphereShape* shapes);
         void SetBoxShapes(ColliderGroup* group, const BoxShape* shapes);
         void SetCapsuleShapes(ColliderGroup* group, const CapsuleShape* shapes);
         void SetHeightmapShapes(ColliderGroup* group, const HeightmapShape* shapes);
        
         Solver* CreateSolver(int max_particles, int max_neighbours, float radius);
         void DestroySolver(Solver* solver);
        
         void GetBounds(Solver* solver, Bounds* bounds);
        
         void SetSolverParameters(Solver* solver, const SolverParameters* parameters);
         void GetSolverParameters(Solver* solver, SolverParameters* parameters);
        
         void AddSimulationTime(Solver* solver, const float step_seconds);
         void UpdateSolver(Solver* solver, const float substep_seconds);
         void ApplyPositionInterpolation(Solver* solver,const float substep_seconds);
        
         void SetConstraintsOrder(Solver* solver, const int* order);
         void GetConstraintsOrder(Solver* solver, int* order);
        
         void SetActiveParticles(Solver* solver, const int* active, const int num);
        
         void SetParticlePhases(Solver* solver,int* phases);
        
         void SetParticlePositions(Solver* solver, float* positions);
        
         void SetPredictedParticlePositions(Solver* solver, float* positions);
        
         void SetPreviousParticlePositions(Solver* solver, float* positions);
        
         void SetRenderableParticlePositions(Solver* solver, float* positions);
        
         void SetParticleRestDensities(Solver* solver, const float* rest_densities);
        
         void SetParticleInverseMasses(Solver* solver, const float* inv_masses);
        
         void SetParticleSolidRadii(Solver* solver, const float* radii);
        
         void SetParticleVelocities(Solver* solver, float* velocities);
        
         void SetConstraintGroupParameters(Solver* solver, const Solver::ConstraintType type, const ConstraintGroupParameters* parameters);
        
         void GetConstraintGroupParameters(Solver* solver, const Solver::ConstraintType type, ConstraintGroupParameters* parameters);
        
         void SetColliderGroup(Solver* solver, ColliderGroup* group);
        
         void SetCollisionMaterials(Solver* solver, CollisionMaterial* materials);
        
         void SetMaterialIndices(Solver* solver, const int* indices);
        
         void SetDistanceConstraints(Solver* solver,
                                     const int* indices,
                                     const float* restLengths,
                                     const float* stiffnesses,
                                     float* stretching);
         void SetActiveDistanceConstraints(Solver* solver,int* active,const int num);
        
         void SetBendingConstraints(Solver* solver,const int* indices,
                                    const float* rest_bends,
                                    const float* bending_stiffnesses);
         void SetActiveBendingConstraints(Solver* solver, int* active,const int num);
        
         void SetSkinConstraints(Solver* solver,
                                 const int* indices,
                                 const Vector3f* skin_points,
                                 const Vector3f* skin_normals,
                                 const float* radii_backstops,
                                 const float* stiffnesses);
         void SetActiveSkinConstraints(Solver* solver, int *active, const int num);
        
        void SetAerodynamicConstraints(Solver* solver,
                                       const int* triangle_indices,
                                       const Vector3f* triangle_normals,
                                       const Vector3f* wind,
                                       const float* aerodynamic_coeffs);
         void SetActiveAerodynamicConstraints(Solver* solver,int *active, const int num);
        
         void SetVolumeConstraints(Solver* solver,
                                   const int* triangle_indices,
                                   const int* first_triangle,
                                   const int* num_triangles,
                                   const int* particle_indices,
                                   const int* first_particle,
                                   const int* num_particles,
                                   const float* rest_volumes,
                                   const float* pressure_stiffnesses);
         void SetActiveVolumeConstraints(Solver* solver,int *active, const int num);
        
         void SetTetherConstraints(Solver* solver,
                                   const int* indices,
                                   const float* max_lenght_scales,
                                   const float* stiffnesses);
         void SetActiveTetherConstraints(Solver* solver,int *active, const int num);
        
         void SetPinConstraints(Solver* solver,
                                const int* indices,
                                const Vector3f* pin_offsets,
                                const float* stiffnesses);
        
         void SetActivePinConstraints(Solver* solver,int *active, const int num);

    }
    
}

#endif
