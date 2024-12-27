USE LeagueDatabase;

START TRANSACTION;

-- pre migration
-- safe the DefaultPenalty values as ints

CREATE TEMPORARY TABLE TmpVoteCategoryPenaltyValues (
	CatId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	DefaultPenalty INT NOT NULL
);

INSERT INTO TmpResultRowsNumbers (CatId, DefaultPenalty)
	SELECT CatId, DefaultPenalty
	FROM VoteCategories;

-- post migration
-- restore DefaultPenalty values as json string

UPDATE VoteCategories `votes` 
	JOIN TmpResultRowsNumbers `tmp` 
		ON `tmp`.CatId=`votes`.CatId
	SET `votes`.DefaultPenalty = CONCAT('{\"Type\": 0, \"Points\": ', tmp.PenaltyPoints, '}');

DROP TABLE TmpVoteCategoryPenaltyValues;

ROLLBACK;