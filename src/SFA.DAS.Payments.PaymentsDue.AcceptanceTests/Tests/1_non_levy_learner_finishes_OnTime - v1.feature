﻿Feature: Non-Levy learner - Basic Day - v1
@NonDas_BasicDay
Background:
	Given the current collection period is R13
	And a learner is undertaking a training with a training provider
	And the SFA contribution percentage is 90%
	And planned course duration is 12 months
	And the following course information:
	| AimSeqNumber | ProgrammeType | FrameworkCode | PathwayCode | StandardCode | FundingLineType                                                       | LearnAimRef | TotalNegotiatedPrice | CompletionStatus |
	| 1            | 2             | 403           | 1           |              | 16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured) | ZPROG001    | 15000                | completed	    |

	And the following contract type 2 On Programme earnings are provided in the latest ILR for the current academic year:
	| PriceEpisodeIdentifier | Delivery Period	| TransactionType | Amount |
	| p1                     | 1				| Learning (TT1)  | 1000   |
	| p1                     | 2				| Learning (TT1)  | 1000   |
	| p1                     | 3				| Learning (TT1)  | 1000   |
	| p1                     | 4				| Learning (TT1)  | 1000   |
	| p1                     | 5				| Learning (TT1)  | 1000   |
	| p1                     | 6				| Learning (TT1)  | 1000   |
	| p1                     | 7				| Learning (TT1)  | 1000   |
	| p1                     | 8				| Learning (TT1)  | 1000   |
	| p1                     | 9				| Learning (TT1)  | 1000   |
	| p1                     | 10				| Learning (TT1)  | 1000   |
	| p1                     | 11				| Learning (TT1)  | 1000   |
	| p1                     | 12				| Learning (TT1)  | 1000   |
	| p1                     | 12				| Completion (TT2)| 3000   |

Scenario: 1_non_levy_learner_finishes_OnTime
	When an earnings event is received
	Then the payments due component will generate the following contract type 2 payments due:
	| PriceEpisodeIdentifier | Delivery Period	| TransactionType   | Amount	|
	| p1                     | 1				| Learning (TT1)	| 1000		|
	| p1                     | 3				| Learning (TT1)	| 1000		|
	| p1                     | 4				| Learning (TT1)	| 1000		|
	| p1                     | 5				| Learning (TT1)	| 1000		|
	| p1                     | 6				| Learning (TT1)	| 1000		|
	| p1                     | 7				| Learning (TT1)	| 1000		|
	| p1                     | 8				| Learning (TT1)	| 1000		|
	| p1                     | 9				| Learning (TT1)	| 1000		|
	| p1                     | 10				| Learning (TT1)	| 1000		|
	| p1                     | 11				| Learning (TT1)	| 1000		|
	| p1                     | 12				| Learning (TT1)	| 1000		|
	| p1                     | 12				| Completion (TT2)	| 3000		|