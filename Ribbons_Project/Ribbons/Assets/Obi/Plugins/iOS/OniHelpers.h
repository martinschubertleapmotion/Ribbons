/*
 *  OniHelpers.h
 *  Oni
 *
 *  Created by José María Méndez González on 21/9/15.
 *  Copyright (c) 2015 ArK. All rights reserved.
 *
 */

#ifndef OniHelpers_
#define OniHelpers_

#include "Dense.h"

namespace Oni
{

    extern "C"
    {
        
        int MakePhase(int group, int flags);
        
        /**
         * Calculates the rest bend factor for a bending constraint between 3 particles.
         * @param coordinates an array of 9 floats: x,y,z of the first particle, x,y,z of the second particle, x,y,z of the third (central) particle.
         */
        float BendingConstraintRest(float* coordinates);
    }
    
}

#endif
