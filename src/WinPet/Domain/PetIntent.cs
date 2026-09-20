namespace WinPet.Domain;

// Shared behavior states; each pet chooses how these look (walking, driving, glowing, etc.).
internal enum PetIntent { Idle, Move, Observe, Rest, Dormant, React, Dragged, Falling }
