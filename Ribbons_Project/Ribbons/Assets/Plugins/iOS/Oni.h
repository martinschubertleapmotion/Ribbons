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
#include "HalfEdgeMesh.h"

#if defined(__APPLE__)
    #define EXPORT __attribute__((visibility("default")))
#else
    #define EXPORT __declspec(dllexport)
#endif

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
        
		EXPORT ColliderGroup* CreateColliderGroup();
		EXPORT void DestroyColliderGroup(ColliderGroup* group);
        
		EXPORT void SetColliders(ColliderGroup* group, const Collider* colliders, const int num);
		EXPORT void SetRigidbodies(ColliderGroup* group, Rigidbody* rigidbodies, const int num);
		EXPORT void SetSphereShapes(ColliderGroup* group, const SphereShape* shapes);
		EXPORT void SetBoxShapes(ColliderGroup* group, const BoxShape* shapes);
		EXPORT void SetCapsuleShapes(ColliderGroup* group, const CapsuleShape* shapes);
		EXPORT void SetHeightmapShapes(ColliderGroup* group, const HeightmapShape* shapes);
        
		EXPORT Solver* CreateSolver(int max_particles, int max_neighbours, float radius);
		EXPORT void DestroySolver(Solver* solver);
        
		EXPORT void GetBounds(Solver* solver, Bounds* bounds);
        
		EXPORT void SetSolverParameters(Solver* solver, const SolverParameters* parameters);
		EXPORT void GetSolverParameters(Solver* solver, SolverParameters* parameters);
        
		EXPORT void AddSimulationTime(Solver* solver, const float step_seconds);
		EXPORT void UpdateSolver(Solver* solver, const float substep_seconds);
		EXPORT void ApplyPositionInterpolation(Solver* solver,const float substep_seconds);
        
		EXPORT void SetConstraintsOrder(Solver* solver, const int* order);
		EXPORT void GetConstraintsOrder(Solver* solver, int* order);
		EXPORT int GetConstraintCount(Solver* solver, const Solver::ConstraintType type);
        
		EXPORT void SetActiveParticles(Solver* solver, const int* active, const int num);
        
		EXPORT void SetParticlePhases(Solver* solver,int* phases);
        
		EXPORT void SetParticlePositions(Solver* solver, float* positions);
        
		EXPORT void SetPredictedParticlePositions(Solver* solver, float* positions);
        
		EXPORT void SetPreviousParticlePositions(Solver* solver, float* positions);
        
		EXPORT void SetRenderableParticlePositions(Solver* solver, float* positions);
        
		EXPORT void SetParticleInverseMasses(Solver* solver, const float* inv_masses);
        
		EXPORT void SetParticleSolidRadii(Solver* solver, const float* radii);
        
		EXPORT void SetParticleVelocities(Solver* solver, float* velocities);
        
		EXPORT void SetConstraintGroupParameters(Solver* solver, const Solver::ConstraintType type, const ConstraintGroupParameters* parameters);
        
		EXPORT void GetConstraintGroupParameters(Solver* solver, const Solver::ConstraintType type, ConstraintGroupParameters* parameters);
        
		EXPORT void SetColliderGroup(Solver* solver, ColliderGroup* group);
        
		EXPORT void SetCollisionMaterials(Solver* solver, CollisionMaterial* materials);
        
		EXPORT void SetMaterialIndices(Solver* solver, const int* indices);
        
		EXPORT void SetFluidMaterials(Solver* solver, FluidMaterial* materials, int num);
        
		EXPORT void SetFluidMaterialIndices(Solver* solver, const int* indices);
        
		EXPORT void SetDistanceConstraints(Solver* solver,
                                     const int* indices,
                                     const float* restLengths,
                                     const float* stiffnesses,
                                     float* stretching);
		EXPORT void SetActiveDistanceConstraints(Solver* solver,int* active,const int num);
        
		EXPORT void SetBendingConstraints(Solver* solver,const int* indices,
                                    const float* rest_bends,
                                    const float* bending_stiffnesses);
		EXPORT void SetActiveBendingConstraints(Solver* solver, int* active,const int num);
        
		EXPORT void SetSkinConstraints(Solver* solver,
                                 const int* indices,
                                 const Vector3f* skin_points,
                                 const Vector3f* skin_normals,
                                 const float* radii_backstops,
                                 const float* stiffnesses);
		EXPORT void SetActiveSkinConstraints(Solver* solver, int *active, const int num);
        
		EXPORT void SetAerodynamicConstraints(Solver* solver,
                                       const int* triangle_indices,
                                       const Vector3f* triangle_normals,
                                       const Vector3f* wind,
                                       const float* aerodynamic_coeffs);
		EXPORT void SetActiveAerodynamicConstraints(Solver* solver,int *active, const int num);
        
		EXPORT  void SetVolumeConstraints(Solver* solver,
                                   const int* triangle_indices,
                                   const int* first_triangle,
                                   const int* num_triangles,
                                   const int* particle_indices,
                                   const int* first_particle,
                                   const int* num_particles,
                                   const float* rest_volumes,
                                   const float* pressure_stiffnesses);
		EXPORT void SetActiveVolumeConstraints(Solver* solver,int *active, const int num);
        
		EXPORT void SetTetherConstraints(Solver* solver,
                                   const int* indices,
                                   const float* max_lenght_scales,
                                   const float* stiffnesses);
		EXPORT void SetActiveTetherConstraints(Solver* solver,int *active, const int num);
        
		EXPORT void SetPinConstraints(Solver* solver,
                                const int* indices,
                                const Vector3f* pin_offsets,
                                const float* stiffnesses);
        
		EXPORT void SetActivePinConstraints(Solver* solver,int *active, const int num);
        
		EXPORT void GetCollisionIndices(Solver* solver,int* indices, int num);
		EXPORT void GetCollisionDistances(Solver* solver,float* collision_distances, int num);
		EXPORT void GetCollisionPoints(Solver* solver,Vector3f* collision_points, int num);
		EXPORT void GetCollisionNormals(Solver* solver,Vector3f* collision_normals, int num);
		EXPORT void GetCollisionNormalImpulses(Solver* solver,float* normal_impulses, int num);
		EXPORT void GetCollisionTangentImpulses(Solver* solver,float* tangent_impulses, int num);
		EXPORT void GetCollisionStickImpulses(Solver* solver,float* stick_impulses, int num);
        
		EXPORT void SetDiffuseParticlePositions(Solver* solver, const Eigen::Vector3f* positions, const int num);
        
		EXPORT void SetDiffuseParticleVelocities(Solver* solver, Eigen::Vector3f* velocities);
        
		EXPORT void SetDiffuseParticleNeighbourCounts(Solver* solver,int* neighbour_counts);
    

    }
    
}

#endif
