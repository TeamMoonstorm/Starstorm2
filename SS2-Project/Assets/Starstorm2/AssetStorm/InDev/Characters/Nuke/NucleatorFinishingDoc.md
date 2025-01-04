# Nucleator finishing Document

This document will detail the stuff that nucleator requires for it to be "feature complete"

# Textures
Nucleator is UV-Unwrapped but has no textures to speak of, refer to goodguy's design for colors and the like

# Animation Controller

Some of the animation controller is implemented such as the movement layers, however, he lacks any kind of override or full body overrides.

Currently the following skill animations are not implemented:

* Secondary
* Alt Secondary
* Utility

I greatly regret not just copying an existing controller...

# Animations

Most animations are going to be blend-tree hell, they have Charge and Fire animations. Both animations have "Low" and "High" states, depending on how much charge nucleator has.

The backpack and the geiger counter should interact with the charge as well. since thats what people will see most of the time it should be used for actual information n such.

# Sound effects

There are no proper nucleator sound effects, its all re-used SFX.

Nucleator CANNOT ship without a geiger clicking being tied to his charge states.

# Charge mechanic

The charge mechanic "Works", i know it does but its a bit of a mess currently. but i dont think it requires refactoring...?

Basically:

* Primary, Secondary, and Utility skills enter into a skill that inherits from a base nuke charge state.
* Charge states have serializedField float fields that determine the base charge, the soft cap and the hard cap
    * Keep in mind, charge is also used as the DamageCoefficient.
    * Soft cap is when nucleator starts taking damage
    * HardCap is when nucleator is taking the most self-damage and is forced to fire.
* Charge states inform the SelfDamageController, which takes care of:
    * Make nucleator take damage
    * Remap the charge state's values into 0-1, used for animations and the crosshair
    * give the remaped value to the animator for animations.
* When a charged state fires, it transitions into a "FireState"
    * The fire state has a reference to the total amount of charge the charge state accumulated.
    * Fire states should interact with the charge in some shape or form:
        * Primary sludge causes the ball to go faster
            * (I'd like to make the homing of it stronger as well with charge)
        * Secondary increases radius of the blast.
        * Flamethrowers get increased length
        * Utilities get increased distances.

# Code

All public facing code has been documented with XML.

# Going forward

Ideally i wouldnt like nucleator to change too much on his current skills, as most of them already work and may just require tweaking. Please while i'm away keep talking on his respective channels so i can chime in and speak about any possible changes... i love this boy but i couldnt deny the job offer.

# What about [DEMON CORE]?

[DEMON CORE] is on hiatus, he no longer needs to co-release with nucleator, he'll become real eventually. i may work on him during day-offs but i want him to be part of the roster as well.