YOGURT v0.01

this build incorporates a test of the saving / loading system which will be very important for creating a persistent world. right now it only saves / loads the state of Tom & the eggplants.
it's insane how hard this is to implement in unity. 3/4 weeks on this implementation alone.

------------

2/4/2014 the system should now save everything. known bugs:

does not save what hat you're wearing
cannot remove items that were saved into a container

also the speech bubble is wonky right now, don't kill me


---------------------


2/17

 the save system works totally now. there is a persistent world and two rooms to move between.

the interface has been totally refactored to get some of that old school point-and-click vibe. shit is a little wonky still.


2/20

interface should be in a complete state as of now, although not pretty.

---------------

3/7
	beginning elementary work on AI. the character is only able to run through goal-oriented branching routines and is aware of the objects in his world, but will not react to the player or any provocation yet.

3/16
	the AI character is set to pick up the fire extinguisher and then walk over to the eggplant and then use it on the eggplant.
	i've added a mind-reading hat and a speed hat.

new items are breaking save / load. good test case for introducing new items to the system.

--------------

9/1
  I took a break to submit my paper to the astrophysical journal and generally get shit done, and think over my approach to the game.

  I transferred primary development over to my macbook. The environment has been upgraded to Unity 5 Pro with a Nintendo license. I have also set up script to automatically back up the entire project to dropbox every night, and separately every week. There is also an automatic nightly build and backup to dropbox.

  I have also placed the script assets under git management on bitbucket, which is very easy to do on a mac. I never managed to get that working before on my windows box, even though I tried like two or three times, because git on windows is fucking stupid and horrible because doing command-line stuff on windows is like hittig yourself in the balls with a mallet.

  So basically the development environment has been vastly upgraded, and the backups are more robust .

  My current goals, now getting back into the project, are to fix and improve the save / load system. Currently the game crashes on load when moving from the first screen to the second. I intend to fix this, document the system a little better, and perhaps make the code more robust with exceptions.

  The Save / Load routines are not attached to mono behaviors, so they aren't wrapped in all the nice routines of game objects, meaning that the code can make the game crash hard. It is also doing a lot of low-level stuff which makes bugs very likely.


-------------------

12/10/15
    major updates are in the direction of creating the game backbone / flow structure.
    there is an apartment, exterior, and studio. game saving and loading, autosave, title screen all work.

    current development is in the direction of the commercial structure:
    defining commercials, knowing when their conditions are met, and how their completion works in context with the larger game.

    also, UI has been totally overhauled back to buttons floating in the game world. i like this better, it feels
    less janky and more stable, but also there was no need to occupy such a large portion of the game world with a UI bar
    that was imitating UI from e.g. monkey island or such where this interface made a lot more sense.]


-------------------

2(?)/9/16
    resuming development following trip home, tim's wedding, the start of a new quarter, and much chaos.

    working toward defining the commercial system.

3/18/16

    commercial system is mostly in place. working on commercial conclusion, and nice FX and feedback
    for the player in the commercial.

6/5/2016

    resuming development after break mid-quarter (research, teaching, and Kira's game have all slowed down or stopped so now I have free time).

    tweaking base engine feel for throwing, colliding, etc.
    adjusting throw to be "zip" and adjusting zip collision with objects: this involves tweaking physical bootstrapper, physical,
    animation, throwing physics, etc.

    in the process i have discovered and fixed a large number of small issues with the 2.5D physical system. now I feel that everything with
    the system is ironed out. there are no longer hacks and mysterious unknowns in the unity engine-- it all makes sense once i track down every
    last bit of the complex, interconnected system.

    ground colliders can now be much smaller thanks to upgraded base unity engine. navigation is much smoother as a result (less
    pushing things out of the way)! collisions are much closer. objects feel slimmer. less unintended interaction.

    throwing animation is in place and works. i need to make new spritesheets for all affected.  advanced animation system has been tweaked
    and now works a little more rationally: inventory calls it, rather than have it poll inventory constantly. there are fewer dumb pointless
    flags. animation logic is laid out a lot more clearly.


7/7/2016

    i've added much to the flavor and polish. there's now a sleep sequence: cutscenes between each day, with each day tracked, starting new
    days in Tom's bed in the apartment. Persistent world is currently such that the apartment maintains state, the world resets. the full cycle
    of commercial - success - reward - new day is fully working.

    commercials now report various metrics and the events that took place, and transcript.

    save system has been overhauled, simplifying things greatly. basically i moved all the game state data into a separate serializable class,
    so saving / loading is simply a matter of reading or writing this data to disk. I also integrated all these new changes and day system into the
    overall new game / load game structure such that it works.

    there are popups indicating new collected items and such, and items are divided out between fridge, closet and dresser.

    the entire thing is almost all there, but right now I'm working on the achievement system (trying to figure out how it will be structured
    code-wise) and then AI I suppose, before I fill out the world.

1/9/2017

    notes following a major series of backend overhauls, bug fixes, polish, and tweaking
    full game-system is in place and robust to saving, loading, quitting, title screen, etc.
    many numerous small fixes.
    overhaul to commercial system, many UI tweaks,
    added email.

2/7/2017

    most of the dialogue system is worked out: a format for writing navigable dialogue trees with
    options, buttons for generic interactions, portraits, text blitting, text advancement and speeding,
    nimrod, nimrod specifications for random insults and threats.

    this lead to more work on the AI, making it more robust, with more interplay between priorities.
    fixed a major point in awareness to have nearest enemy / threat be a reference object on awareness
    that is updated regularly, thus saving many dozens of identical calls to a function, and simplifying
    the structure. the AI is generally more advanced now. characters recognize when others are knocked
    out and therefore not a threat, etc.

    there are more features to be filled out, like give / take / suggest, or attempting something beyond
    a proof of concept for the nimrod files.

    redrew background graphics for first outside world (rough draft). looks pretty good and chunky
    zoomed in.

5/6/2017

    the first two commercials are in place and work as intended to guide the player through the idea of the
    game.

    i'm working on expanding the world and adding in the next commercials with the intent of
    incorporating the first major "skill jump" for the player, the duplicator

7/15/2017

    tighetned up 2.5D physics : not a tweak, but a revolution in how it's handled.
    the main improvement is the addition of a "horizon" (edgecollider2D) collider attached to the ground object:
    the horizon is kinematic: it does not transmit force to the ground when the main object collides with it.
    this allows it to be a stable, flat ground, meaning even grounded objects can tip over and lie on it.
    this has simplified things and tightened up much of the collisions and dynamics.

    i've added a duplicator and the duplicator commercial cycle: eggplants and 10 eggplants commercials.
    this entailed working up the emails and package delivery subsystems.

    i've been bug fixing, tightening up the general mechanics and world.

    ai has been improved and made more robust (still not totally robust yet!)

    a farmer with a produce stand sells an eggplant in exchange for a dollar; he guards his possession and
    replaces it if it gets moved.

    i now have a plan for how to expand the world, including a bridge guarded by a knight, a deathtrap
    dungeon to introduce the idea of the death exploit; strength potion, etc.

8/6/2017

    after watching evan play the game and showing the game to aidan, i think i can see that
    i want to make the beginning of the game more interesting to grab the player.
    stick/fire commercial is good intro to puzzling but requires a lot of backtracking.
    following that up with eggplant is kinda boring.
    some ideas:
        world will be more immediately interesting shortly
        add some crackpot commercial emails after first day
        make apartment smaller
        add a visitation from yourself from the future with a dire warning

    nevertheless, the game is developing nicely along strong lines. i have added the deathtrap
    cave to introduce the death exploit; this puzzle cycle goes
    deathtrap -> strength potion -> defeat knight -> gain armor / weapon -> defeat deathtrap

    further notes: i want to add record / playback functionality.
    thinking about that makes me think that the save / load isn't quite there.
    therefore i would really like a test system for the save / load functionality.

    how would this work? ideally something like: start, save, load, then compare loaded to start.
    the problem in the comparison is that if my save system is how i can serialize gameobjects, then
    if it misses something on saving it will miss something on loading.

    it needs to compare unity's serialized level to its saved / loaded copy.

    so maybe something like:
    it loads test level.
    upon loaded, save, then load, but deserealize "next door" and see how they both play out

    that would be ideal, but since they run simultaneously, random seed could not be shared.
    furthermore, there could be bleed between the worlds and they might influence one another.

    the point, again, is to compare my save / load with unity's. this may only b

8/20/2017

    i tweaked the AI handlers and now save / load feels a lot more stable.
    there still feels like race conditions in all the ordering of level loading and all that.
    also, anything on a table doesn't save / load properly.

9/5/2017

    i figured out color theory for better art for the forest level. i will adapt this technique to the other
    levels, and this frees up my conscience to spend more time drawing levels, knowing that they ought to come
    out better.
    this is a pretty big relief / discovery / improvement.

    i have also greatly improved the save system, mostly just by adding handlers and such. there are debug
    buttons for saving / loading at will. loading is vastly improved and everything feels a lot more stable
    as a result. I did not realize that the only parameter previously saved for physicals is "doInit"! now we save height,
    mode, position, rotation, everything quite precisely. Saves and loads back in the same position.

    the solidity of the save system inspired such confidence that i started thinking seriously about a record / playback
    feature. it would be fairly trivial to implement a keypress recorder

    i also figured out a way to test if the object collider overlaps the horizon and adjust the slider limits
    accordingly. this prevents the self-propelled sliding ground objects.

    note: to fix some hand item button problems on new scene / load: do we do a final button check after
    all the load inits are processed? it should look like:

    1. clean scene
    2. load scene objects
    3. load player objects
    4. set focus
    5. broadcast loadinit
    6. check hand buttons

    i think the actual details are different, (loadinit comes earlier?) still need to poke around

10/1/2017

    first day of October!!!

    added a knight to the forest that guards the bridge. this required new things:
        AI guarding / warning behavior
        armor
        helmets (a type of hat that replaces head graphic)
        holding angle offset (so the sword does not run into the helmet)s

    cutting weapons (swords) draw blood and do not knock unconscious (they kill)

    added strength potion & strength intrinsics
        liquid containers impart intrinsics (this needs to change)

    fixed a bug with AI controllable directions being set to non-normalized vectors by AI routines.

    to do:
        something must introduce the player to the mechanic of taking clothes (fireman commercial? send email then?)
        intrinsics tied to liquid, not container
            refactor liquid loading and specification
        perks
        retain knight defending bridge on save
        • fix walking while doubled-over / stunned
        • use friction to reduce floating upon knockdown / stun
        • fix speech text going off the screen
        needs a more sophisticated and consistent armor / strength algorithm:
            armor repels attacks with stick / punch, negated by strength
            armor can survive the arrow trap
            strength changes punch
        strength impact FX
        make vomiting even more disturbing if it's vomited-up offal / immoral foods
        add more reactions to eating bad things, or the player character witnessing things

    damage taking needs to be refactored:
        hurtable, physical, tree, and destructuble all duplicate the same impact callback logic.
        now that logic needs to be a little more complex because of repel sound, and strength attack.
        it would make sense if there was IDamageable but that defeats the purpose of having messages, right?
            messages: reduce the coupling between components. be agnostic about what components exist on an object.
                      liquidcontainer takes the message and spills.
        but damageable is different: the message can be handled generally by multiple components, each doing a specific thing
        damageable, though, requires feedback to the user: it must be tied to SFX.
        now, if it is an interface, lots of logic will be repeated, right?
        this is the crux of it: lots of things can react to damage messages in general, but
            things that take damage must provide SFX feedback to player. that needs to be handled generally.
            repel: repel sfx. else: normal SFX unless strength, then strength SFX + explosion + delay or whatever
        but, each thing will process damage its own way. each thing needs to flag back to the thing if it repeled the attack.
        i feel like each damagable should have a "takedamage" method that returns what type of impact (repel, norm, strong) and
            this must be called by physicalimpact (or anything else that does physical damage!)
        it's like there should be a class of objects that all handle damage the same way (subclass messageable interface?)

        the whole idea is not to repeat code.
        code is repeated when damageable things all take a damage message and do a takedamage routine and they all repeat the same
            way of calling the physical impactor's FX function.
            calling the FX code should not be optional. if something takes damage, it must be compelled to produce an impact effect,
                and really, this should be handled by the physical impactor.
        maybe takedamage belongs to message damage.
        the information goes inv -> slash -> physical impact -> damage message -> damageable ->



10/10/2017

    refactored many things, including hurtable.
    liquids can have intrinsics attached. liquid spec and loading is greatly simplified. no more editing xml files by hand.
    spilled potion can impart effects.
    strength effect
    strength impact effect
    armor algorithm taking armor and strength into account
    blood, cutting, damage types, etc.
    stains carry over from scene to scene

    combat has been added to game, and killing.
        knight can defeat knight
        cutting weapons do not draw blood if they are repelled

    current rundown of the damage system:
        baseball bat: 38
        stick: 30
        sword: 38
        punch: 20
        dart: 25

        strong:
            ignore armor

        armor > punch amount
        armor > stick amount
        armor > dart amount
        sword > armor amount

        √ new vals:
        punch: 10
        stick: 20
        dart: 30
        armor: 35
        baseball bat: 30
        sword: 40



10/19

    corpse gibbing is implemented. can obtain human hearts, etc.

    todo:
        gibbing to physical transforms...?
            initial height
            initial velocity and trajectory
        characters apprehending you covered in liquids
        recording commercial while covered in blood
            commercials record what you are wearing / appearance: fireman commercial
        death effect? ghost flying up? ghost can be captured?
        witnessing disturbing or disgusting events: more reaction?
            vomiting
            running away
            exclamations
            specific vocal responses drawn from a nimrod bank.
        fire ext. coats target in white
        √ dropdripper handler
        slowdown on lots of gibs
            ? reproducible?
        status effects indicator
        vomiting foods fix
        kinetic liquid fill?
            level?
            sloshing?
            gibs floating inside?
        √ spice up email about duplicator
            scrolling email dialog menu
        eating head, arms, etc. leaves skulls, bones
        unfazed by death personality trait?

    COMMERCIAL ANALYTICS SAMPLE TEXT
        describing sentences that should be possible, that the player ought to see during normal
        gameplay.
        source snippets for later nimrod definitions.

        "i don't understand why it had to have _____"
        "the commercial prominently featured corpse desicration"
        "I didn't like the part where _____"
            e.g., vomiting
                vomiting up blood / body part?
                murder "___ murdered ___ with ____"

        √ a highly disturbing commercial that prominently features violence, yogurt eating, and insults

        defines convention by including
        featuring
        including
        that relies on
        that makes (startling, shocking) use of
        centered around



11/5/2017

    event system has been completely overhauled and standardized. massive changes to the underlying data structures.
        • commercial ratings, awareness reactions, commercial requirements are all a single unified structure
        • different instances of the same event will have the same properties as they use static factory methods for their
            definition

    a working proof of concept of commercial analytics is in place. commercials can be described and summarized.
    focus groups can pull out specific outlier events for comment.

    now working on:
    an overhaul / standardization / normalization of ratings system. everything will be greatly simplified to a
    three-point system (could be expanded later). this enables:
        • easier way to guage the scale of particular ratings
        • easier to use proper strength descriptors for events (mild medium hot)
        • easier to choose appropriate reaction to event
    i also think that occurrences should be uniqued in the final step when calculating total values and such.
    this prevents achieving arbitrary score by just repeating a single act over & over, and it feels like it makes
    the analysis more consistent or something.... makes the final product more repeatable, more consistent?

    potential problem: the top ___ lists will include a lot of ties... but is it important? probably not that
    important. the actually stand-out events will stand out.

    does it make sense to make icons for the different ratings? show symbols when that event occurs?
        world symbols: an option to toggle? too busy? i can try it and see

    0: not notable
    1: present, but appropriate, socially acceptable
        could be seen on Full House
    2: extreme for TV, not for polite company
        could be seen on HBO
    3: way over the line, socially unacceptable
        would not be allowed on TV
        breaches FCC rules
        might be treated as as provocation
    4: extreme for real life / possibly illegal
        could prompt investigations or arrests
        not expected

11/8/2017

    BUFF OVERHAUL
        simplified and better designed code for buffs and intrinsics.
        saving/loading improved, simplified.

    bug: hats are the only thing not working

    internal bleeding: vomit blood (after drinking drain cleaner?)
        announce "i am vomiting blood!"
        internal bleeding, poisoning, as status icons? "buffs" in that sense?
            it would be easy to confer poisoning from liquids / foods if that was an intrinsic: the infrastructure is in place.

    next steps in world:
        strength lets you pick up videocamera. bring it to the mayor's house to put the mayor on camera.
        mayor's house is on the other side of the bridge.
        also: new mechanic: hypnosis. bring the other character to the camera to shoot.

        perhaps: bring videocamera over the volcano; hypnotize the mayor across the bridge?
            because an ordinary character would get killed by the volcano? but player can dress up as fireman
            and carry the videocamera across?

        1. relocatable video camera
        2. hypnosis
            is it a skill? or an item?
            if it is an item: how do you return to Tom?
                I kind of envision it as you take over the one, everything is identical (no "return" button)
                and you must walk back to Tom to switch back.
                this feels like a skill- maybe used in talking.
                talking is good: it allows the skill to be limited to talking range. only people that can talk and
                    want to talk.
                problem: burying it in the dialogue menu makes it difficult to introduce.
                    i like the idea of introducing new buttons to the dialogue menu as the player gains skills.
                    i don't like burying hypnosis behind dialogue menu
                add new action button to top of screen
                    but then how is this skill limited? distance? leave this to later?
                    when in take-over mode, highlight characters? distance radius?

11/22/2017
    save overhaul:
        i simplified a lot of the code
        removed a lot of redundant things, like referenceresolver
        separated "resolvereference" and "referencetree" functionality
            knowing about an object does not mean it is part of your reference tree
    hypnosis effect
        nicer penner function smoothing on camera movement
    when does player get the lighter?
    blood spray from impact better
    object trajectory from impact better

11/29
    √ fix the intrinsics saving
    √ fix stain saving
    √ fix costar line
    √ fix apartment saving / loading
    √ fix vomiting!
    √ fix piggybank
    √ better action buttons handling on switching / inventory, etc
    √ inventory menu
    √ bones left over
    √ offensive_4
    right click to pick up / drop / throw?

12/2/2017
    "buff up the items initiative"
        * pick up sounds
        * impact sounds
        * ground landing sounds
        * gibs: physical, fire, cutting
        * descriptions
        * damage and breakability

    tables, drop collisions
        droplet spray seems to push the blender around, which shouldn't happen
        drops push each other around a little bit
        √ some drops don't splash on the ground like they should
        blender spraying on table does not exhibit expected behavior
        √ spilling is weird

    portrait of Tom

    √ better handling of new gameobject IDs?

    √ fix knight saving / loading
        seems to lose the plot after save / load re: priorities
        √ long-term memory storage for bridge
            part of IDToGameObject and GameObjectToID: storing the references, esp. if they dont get resolved.
        √ something like: a global storage of item states
            refer back to that global storage instead of saving levels
                levels could be containers of persistentobjects that have ids and things

12/4/2017
    defended my PhD thesis

12/6/2017
    new save overhaul! SAVE v0.3
        global object data storage
        levels store list of objects / prefab names and positions etc.
    the persistentobjects are only instantiated when not in database
    the persistentcomponent is only instantiated when the persistentobject is new
    all other times, we are dealing with an existing instance which is in the global database.

    RESULTS
        update vs. overrwrite GO references is handled inside a function which is used globally
        trees, loaded, saved objects lists are all much simpler
        levels and players are much simpler
        there is a single repository of state
        handlers are removed. functionality moved to ISaveable interface.
        moving the object database to ram means i don't have to save or load it on frame change
        objects are garbage collected if they were left outside on save
            (fix this if it applies to Save & Exit)

12/9/2017
    let's think about how the game is developing
    it feels like there's still an issue with getting the player to progress.
        1. reward for completing commercials
            right now, their main thing is that they unlock other commercials
            but the player unlocks abilities in order to beat the commercial
        2. shooting a commercial is not challenging or that interesting yet
        3. a player may need to know they need item/ability A without knowing they need to do steps B, C, D, E to get A.
        4. show off AI chops more. more AI-AI interactions.
    think of a metroidvania. the player gains abilities through exploration. their ultimate goal is to get to the end to beat
    the final boss. progress is provided by changing levels / minibosses
        what motivates the player is metered exploration. perhaps that carries over here
        the commercial anchor, though, starts to make less sense
        but it makes more sense if you must take the camera out
            a line of commercials that involve taking the camera out
            start with simple and easy commercials. feed yogurt to farmer.
                a dialogue interface for feeding yogurt or making suggestions?
                other ways to feed yogurt to noncooperative things?
    MORE FLAVOR
        something pops into apartment randomly?
            peter picklebottom collects old items
        different people outside
        more emails
        outside world should change a little bit from day to day
        a new apartment gets added onto yours, making a duplex
            long dialogue with apologetic man earthbound style
            upstairs neighbors?

    SCHEME
        lay out a bunch of obstacles
            identify what abilities could defeat them
            there may be several layers of difficulty
            visualize how a growing ability set defeats increasingly more obstacles
                something like venn diagrams?
                meter out the rewards accordingly

    <moved to world_design_plan.txt>

12/10/2017
    before i move much further on content, i need to be able to map state
    and answer some basic questions about how the map functions.

    first, try to map the present game.

    i have determined that the problem is a path in a graph

    i have determined a way to map out a series of puzzles, tied to zones,
    which unlock abilities

    obstacle:
        accessible
        eclipsed
        can_be_defeated(state)

12/15/2017
    i have landed on a solution to the puzzle order problem.
    it is a method for constructing a directed graph of obstacles and ability unlocks such that the graph can be navigated to the end
        prevents impossible puzzles
        prevents unlocking stronger abilities first
    this is good, now i just need to implement it.

    solution is fully implemented and i have described the current state of the game.

12/16/2017
    i have used my new model to discern what can be safely added next
    i'm also moving in the direction of the vampire
        you must eat the vampire's heart to gain vampirism

12/23/2017
    took a short break after successfully working out the puzzle planning

    (?: maybe) advanced animation: split into head / torso / legs ?
    √ better vomit effect
        food, liquid come from mouth

    <moved to world_design_plan.txt>

12/24/2017
    new controllable type
        what's going on between inventory, controllable.defaultinteraction, and control?
        simpleControl: a parent class that contains humanoid-style propulsion based on player input.
            humanoid inherits from this, and adds specific stuff for inventory handling & animation & etc
            peter picklebottom uses this, for its simplicity
    peter picklebottom is added
    potential issues:
        many items over the limit makes for a long cutscene
            give player control after a moment or n items collected?
                multiple PPBs in the house at once?
            speed up with each item?
        there should be a way to trap / hypnotize PPB.

    (?) change feel: faster, faster animation, greater acceleration / deceleration ? 
        maniac mansion vs. police quest
    
    


1/3/2018
    overhauled damage: made damageable an abstract class and immunities are handled on this level
    immunity, impacts, sound effects are handled on the parent (abstract) class level
        (so now, damageable and hurtable & etc. are treated exactly the same when it comes to immunities & intrinsics)

    rule of thumb:
        use Awake() for all runtime initializations. especially things 
            that will be referenced on Save or Load.
        do not send messages during awake, as the receivers are not guaranteed to be initialized.
    
    note:
        √ fix: save at death / or: picking something up before you hurl into the volcano
        the problem is that state doesn't fully reset, but the tree is not populated before death
        this leads to components saving references to items that are not saved in the tree
            it is handled gracefully but is still a problem
        the stuff that needs to persist is stored in memory in gamedata and persists into new day

    
    

1/14/2018
    added cannon, adding teleporter
    several peter picklebottom fixes

    √ better camera control
        √ when the whole thing can't fit, center it
        how to arrange camera in morning cutscene?
    √ new day collected items etc
    √ weird costars in cutscene
    if (dam.type == damageType.fire || dam.type == damageType.cosmic || dam.type == damageType.asphyxiation)
        * destructible
        * damageable
        * physical
        this is a bit of code smell. probably there needs to be a struct that includes type and whether or not it's impactful,
            or that needs to be defined for each type of damage somewhere
    punch button does nothing

    
1/21/2018
    a week of bugfixes. added moon, moon cave, several achievements, some more overhauls

    remember the other main design goal: unexpected interactions and combinations
    can we get teleporter before the dungeon? doesn't it defeat dungeon without smuggling in stomach?
        nuclear option: teleporter does not allow you to bring items (bad, defeats purpose, unclear to player)
    the dungeon will start by dropping the player through an inv. stripper mechanism.
    more items
    cutscene more animated
    add more sounds to the UI
    more AI-AI interaction

