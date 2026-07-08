// =====================================================================
// The UNI-verse — Chapter 1: The First Academic Year
// Serious-game academic decision loop (§4.5.2)
//
// This story runs inside the isolated Ink virtual machine. The Unity side
// (StoryManager) injects the variables below on scene load and keeps them
// synchronized through the double-entry sync pattern (§3.6). The narrative
// mutates game state exclusively through metadata tags:
//
//   #speaker: Name        dialogue name flag
//   #credits: +10         award/deduct academic credits
//   #stat: academic +8    adjust a stat (social/academic/health/stress/motivation)
//   #stress: +5           shorthand stat tags
//   #motivation: -10
//   #attend / #skip       lecture attendance (feeds the will-I-passometer)
//   #background: id       environmental cue
//   #audio: id            audio backdrop cue
//   #chapter_end          run the credit-threshold gate (§2.7)
//
// Balance note: 70 credits are obtainable, 60 are required — consistent
// attendance matters, and one large slip usually costs the year. Tune the
// deltas freely; the C# side clamps and validates everything.
// =====================================================================

// ---- Globals injected & synced by the GameManager (§3.3) ----
VAR player_name = "Player"
VAR chapter = 1
VAR total_credits = 0
VAR credits_required = 60
VAR academic = 50
VAR social = 50
VAR health = 50
VAR stress = 25
VAR motivation = 75
VAR support_unlocked = false
VAR passometer = 50

-> orientation

// ---------------------------------------------------------------------
=== orientation ===
The first Monday of the academic year. The lecture hall smells of fresh paint and nervous ambition. #speaker: Narrator #background: campus #audio: day
Somewhere between the coffee queue and the timetable board, Amaara finds you. #speaker: Narrator
There you are, {player_name}! Can you believe it? Week one of Computer Science. #speaker: Amaara
I checked the credit rules: we need {credits_required} credits by June, or we repeat the whole year. No pressure, right? #speaker: Amaara
-> week_one

// ---------------------------------------------------------------------
=== week_one ===
Tuesday, 7:40 AM. Advanced Data Structures starts in twenty minutes, on the far side of campus. #speaker: Narrator
* [Attend the lecture]
    You claim a front-row seat and take notes until your hand cramps. #speaker: Narrator #attend #credits: +10 #stat: academic +8 #stress: +5
    Professor Ionescu nods at your questions. A strong start to the semester. #speaker: Narrator
* [Skip it and sleep in]
    You silence the alarm. The extra sleep is glorious... and expensive. #speaker: Narrator #skip #stat: academic -5 #motivation: -10 #stress: -10
    Amaara forwards you her notes with a single raised-eyebrow emoji. #speaker: Amaara
- -> lab_day

// ---------------------------------------------------------------------
=== lab_day ===
Wednesday is laboratory day: your first programming assignment is due at midnight. #speaker: Narrator #background: lab
Amaara is already at a workstation, two energy drinks deep. #speaker: Narrator
So. Assignment, or that new coffee shop we found? Choose wisely. #speaker: Amaara
* [Finish the assignment properly]
    Four hours of segmentation faults later, the tests finally pass. #speaker: Narrator #credits: +15 #stat: academic +6 #stress: +10
    Submitted at 22:47. You sleep like a compiler at rest. #speaker: Narrator
* [Rush it and submit late]
    You cobble something together at 1 AM. Half the tests glow red. #speaker: Narrator #credits: +8 #stat: academic -3 #stress: +10 #motivation: -15
    The late-submission penalty stings more than you expected. #speaker: Narrator
* [Ditch it for the coffee shop]
    The cappuccino is perfect. The deadline passes somewhere between laughs. #speaker: Narrator #stat: social +10 #motivation: +10 #stress: -10
    It was a wonderful evening... you just hope it wasn't a costly one. #speaker: Narrator
- -> mid_semester

// ---------------------------------------------------------------------
=== mid_semester ===
Midterm season arrives like weather: everyone saw it coming, nobody is ready. #speaker: Narrator #background: library #audio: night
{ motivation <= 30:
    Lately, opening the laptop feels like lifting a piano. Something has to give. #speaker: Narrator
}
-> options
= options
* {support_unlocked} [Visit the student support counselor]
    The counselor listens without judgement and helps you rebuild your schedule. #speaker: Counselor #stat: motivation +25 #stress: -20
    You leave the office feeling like the semester might be survivable after all. #speaker: Narrator
    -> options
* [Join Amaara's study group]
    Flashcards, whiteboards, and someone always explaining recursion wrong first. #speaker: Narrator #credits: +12 #stat: academic +6 #stat: social +5 #stress: +5
    Teaching each other turns out to be the best revision there is. #speaker: Amaara
    -> seminar
* [Cram alone, all night, every night]
    Just you, cold pizza, and four hundred slides at 3 AM. #speaker: Narrator #credits: +12 #stat: academic +5 #stress: +20 #motivation: -20
    You pass the midterms, but the mirror shows a ghost with your haircut. #speaker: Narrator
    -> seminar
* [Take the week easy]
    Long walks, early nights. Your mind thanks you; your transcript does not. #speaker: Narrator #stress: -15 #motivation: +5
    -> seminar

// ---------------------------------------------------------------------
=== seminar ===
A visiting engineer hosts an optional seminar: "How Real Systems Fail". #speaker: Narrator #background: campus #audio: day
Optional... but the credit catalogue says it counts. #speaker: Amaara
* [Attend the seminar]
    War stories about production outages at 2 AM. You take three pages of notes. #speaker: Narrator #attend #credits: +8 #stat: academic +4 #stat: motivation +5
* [Skip it — it's optional]
    You spend the afternoon gaming. The guilt patch arrives around dinner. #speaker: Narrator #skip #motivation: -10
- -> exam_week

// ---------------------------------------------------------------------
=== exam_week ===
June. The corridor outside the exam hall hums like a server room. #speaker: Narrator #background: exam_hall #audio: night
You have {total_credits} of the {credits_required} credits you need. The final exam is worth the rest. #speaker: Narrator
{ stress >= 70:
    Your hands won't quite stop shaking. This is what running at 99% CPU feels like. #speaker: Narrator #stat: health -5
}
{ motivation <= 25:
    A dark thought keeps looping: what if you were never cut out for this? #speaker: Narrator
}
-> exam_options
= exam_options
* {support_unlocked} [See the counselor before the exam]
    Ten minutes of breathing exercises and a quiet pep talk. #speaker: Counselor #stat: motivation +20 #stress: -15
    Whatever happens in there, it does not define you. #speaker: Counselor
    -> exam_options
* [Enter the exam hall]
    You find your seat and turn over the first page. #speaker: Narrator #attend
    { academic >= 60:
        Question after question looks... familiar. Your preparation carries you through. #speaker: Narrator #credits: +25 #stat: motivation +10
    - else:
        { academic >= 45:
            You scrape through on partial answers and educated guesses. #speaker: Narrator #credits: +15 #stress: +10
        - else:
            The page might as well be written in Klingon. You salvage what you can. #speaker: Narrator #credits: +8 #stress: +15 #stat: motivation -15
        }
    }
    -> results

// ---------------------------------------------------------------------
=== results ===
Results day. The end-of-year board convenes behind a heavy oak door. #speaker: Narrator #background: campus #audio: day
You earned {total_credits} of the {credits_required} required credits this year. #speaker: Narrator
{ total_credits >= credits_required:
    The registrar looks up and smiles: "Congratulations. Year 2 awaits you." #speaker: Registrar #stat: motivation +10
    Outside, Amaara tackles you with a hug. You actually made it. #speaker: Amaara
- else:
    The registrar shakes her head slowly: "Below the threshold. The year must be retaken." #speaker: Registrar
    The campus seems to reset around you — the same first Monday, the same fresh paint. But this time, you remember everything. #speaker: Narrator #audio: night
}
This chapter of the UNI-verse closes here. #speaker: Narrator #chapter_end
-> END
