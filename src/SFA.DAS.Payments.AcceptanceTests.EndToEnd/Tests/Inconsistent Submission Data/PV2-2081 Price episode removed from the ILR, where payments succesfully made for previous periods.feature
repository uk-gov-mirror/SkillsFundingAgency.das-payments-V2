Feature: PV2-2081-Price-episode-removed-from-ILR
Scenario: PV2-2081 Payments made for Price episode during previous collection periods however Price episode is now removed from Latest ILR

Given Commitment exists for learner in period R05
When an ILR file is submitted for period R05
And After Period-end following provider payments will be generated in database
    | Learner ID | Collection Period         | Delivery Period           | Levy Payments | Transaction Type | PriceEpisodeIdentifier |
    | learner b  | R05/Current Academic Year | Aug/Current Academic Year | 305.88235     | Learning         | 25-102-05/08/2020      |
    | learner b  | R05/Current Academic Year | Sep/Current Academic Year | 305.88235     | Learning         | 25-102-05/08/2020      |
    | learner b  | R05/Current Academic Year | Oct/Current Academic Year | 305.88235     | Learning         | 25-102-05/08/2020      |
    | learner b  | R05/Current Academic Year | Nov/Current Academic Year | 305.88235     | Learning         | 25-102-05/08/2020      |
    | learner b  | R05/Current Academic Year | Dec/Current Academic Year | 305.88235     | Learning         | 25-102-05/08/2020      |
When an ILR file is submitted for period R06
Then After Period-end following provider payments will be generated in database
    | Learner ID | Collection Period         | Delivery Period           | Levy Payments | Transaction Type | PriceEpisodeIdentifier |
    | learner a  | R08/Current Academic Year | Aug/Current Academic Year | -50           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Sep/Current Academic Year | -50           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Oct/Current Academic Year | -50           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Nov/Current Academic Year | -50           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Dec/Current Academic Year | -50           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Jan/Current Academic Year | 550           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Feb/Current Academic Year | 550           | Learning         | 25-102-01/09/2020      |
    | learner a  | R08/Current Academic Year | Mar/Current Academic Year | 550           | Learning         | 25-102-01/09/2020      |


