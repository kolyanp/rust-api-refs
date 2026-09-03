using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class SinglePlayerDartsGameController : IDartsGameController, IDisposable
{
	public DartsGameBoard Board { get; private set; }

	private bool IsServer => Board.isServer;

	private bool IsClient => Board.isClient;

	public IDartsGameController.DartsGameState State { get; private set; }

	public bool HasGameInProgress => State >= IDartsGameController.DartsGameState.PreGame;

	public bool IsGameStarting => State == IDartsGameController.DartsGameState.PreGame;

	public bool IsGameOngoing => State == IDartsGameController.DartsGameState.InGame;

	public bool IsGameEnding => State == IDartsGameController.DartsGameState.PostGame;

	public List<DartsPlayerData> PlayerData { get; private set; }

	public int activePlayerIndex => 0;

	public DartsPlayerData GetActivePlayerData()
	{
		return PlayerData[0];
	}

	public bool CanPlay(BasePlayer player)
	{
		if (HasGameInProgress && GetActivePlayerData().HasUser)
		{
			return GetActivePlayerData().UserID == (ulong)player.userID;
		}
		return true;
	}

	public bool IsAtBoard(BasePlayer player)
	{
		if ((Object)(object)Board.mountable != (Object)null)
		{
			return (Object)(object)Board.mountable.GetMounted() == (Object)(object)player;
		}
		return false;
	}

	public bool IsPlayersTurn(BasePlayer player)
	{
		if (HasGameInProgress)
		{
			return GetActivePlayerData().UserID == (ulong)player.userID;
		}
		return false;
	}

	public bool IsInGame(BasePlayer player)
	{
		return IsInGame(player.userID);
	}

	public SinglePlayerDartsGameController(DartsGameBoard board)
	{
		Board = board;
		State = IDartsGameController.DartsGameState.NotPlaying;
		PlayerData = new List<DartsPlayerData>();
		PlayerData.Add(new DartsPlayerData(IsServer, 0));
	}

	public bool IsInGame(ulong userID)
	{
		return GetActivePlayerData().UserID == userID;
	}

	public void Dispose()
	{
		State = IDartsGameController.DartsGameState.NotPlaying;
		GetActivePlayerData().Dispose();
	}

	public void Save(DartsGame syncData)
	{
		if (IsServer)
		{
			GetActivePlayerData().Save(syncData);
			syncData.state = (int)State;
		}
	}

	public void JoinGame(BasePlayer player)
	{
		JoinGame(player.userID);
	}

	public void JoinGame(ulong userID)
	{
		if (!IsInGame(userID))
		{
			GetActivePlayerData().JoinGame(userID);
			StartPreGame();
			Board.SendNetworkUpdate();
		}
	}

	public void LeaveGame(BasePlayer player)
	{
		LeaveGame(player.userID);
	}

	public void LeaveGame(ulong userID)
	{
		if (IsInGame(userID))
		{
			EndGame();
			GetActivePlayerData().LeaveGame(userID);
			Board.SendNetworkUpdate();
		}
	}

	public void ForceLeaveGame()
	{
		if (GetActivePlayerData().HasUser)
		{
			LeaveGame(GetActivePlayerData().UserID);
		}
	}

	public void StartPreGame()
	{
		Board.DartsDebug("[DartsGameController] Pre game starting");
		State = IDartsGameController.DartsGameState.PreGame;
		GetActivePlayerData().State = DartsPlayerData.DartsPlayerState.StartingGame;
		Board.NewTurn();
		StartGame();
		Board.SendNetworkUpdate();
	}

	public void StartGame()
	{
		Board.DartsDebug("[DartsGameController] Game starting");
		State = IDartsGameController.DartsGameState.InGame;
		GetActivePlayerData().State = DartsPlayerData.DartsPlayerState.InGame;
		Board.SendNetworkUpdate();
	}

	public void StartPostGame()
	{
		Board.DartsDebug("[DartsGameController] Post game starting");
		State = IDartsGameController.DartsGameState.PostGame;
		GetActivePlayerData().State = DartsPlayerData.DartsPlayerState.EndingGame;
		DartsGameLeaderboardEntry val = Pool.Get<DartsGameLeaderboardEntry>();
		val.userid = GetActivePlayerData().UserID;
		BasePlayer basePlayer = BasePlayer.FindByID(GetActivePlayerData().UserID);
		string playerName = "";
		if ((Object)(object)basePlayer != (Object)null)
		{
			playerName = StringEx.EscapeRichText(basePlayer.displayName, false);
		}
		val.playerName = playerName;
		val.dartsThrown = GetActivePlayerData().DartsThrown;
		val.timeTaken = GetActivePlayerData().TimeTaken;
		DartsGameBoard board = Board;
		if (board.Leaderboard == null)
		{
			board.Leaderboard = new List<DartsGameLeaderboardEntry>();
		}
		Board.Leaderboard.Add(val);
		Board.Leaderboard.Sort((DartsGameLeaderboardEntry x, DartsGameLeaderboardEntry y) => (x.dartsThrown.CompareTo(y.dartsThrown) == 0) ? x.timeTaken.CompareTo(y.timeTaken) : x.dartsThrown.CompareTo(y.dartsThrown));
		while (Board.Leaderboard.Count > 5)
		{
			int index = Board.Leaderboard.Count - 1;
			DartsGameLeaderboardEntry obj = Board.Leaderboard[index];
			if (obj != null)
			{
				obj.ResetToPool();
			}
			Board.Leaderboard.RemoveAt(index);
		}
		Board.DartsDebug($"[DartsGameController] Player {val.playerName} finished game with {val.dartsThrown} darts thrown and time taken {val.timeTaken}");
		Board.DartsDebug($"[DartsGameController] Leaderboard now has {Board.Leaderboard.Count} entries");
		Board.SendNetworkUpdate();
	}

	public void EndGame()
	{
		Board.DartsDebug("[DartsGameController] Ending Game");
		State = IDartsGameController.DartsGameState.NotPlaying;
		GetActivePlayerData().LeaveGame(GetActivePlayerData().UserID);
		Board.SendNetworkUpdate();
	}

	public void ServerReceivedPlayerDartThrow(BasePlayer player, int points, int pointsModifier)
	{
		if (!IsInGame(player) || !CanPlay(player) || !IsAtBoard(player))
		{
			return;
		}
		DartsPlayerData activePlayer = GetActivePlayerData();
		activePlayer.DartsThrownThisTurn++;
		activePlayer.DartsThrown++;
		if (points == DartsGameBoard.BullScore && pointsModifier == 2)
		{
			Board.SendBullseye();
		}
		bool newTurn = activePlayer.DartsThrownThisTurn == 3;
		bool bustedTurn = false;
		activePlayer.ScoreThisTurn += points * pointsModifier;
		if (activePlayer.Score + activePlayer.ScoreThisTurn > Board.scoreTarget)
		{
			Bust();
		}
		else if (activePlayer.Score + activePlayer.ScoreThisTurn == Board.scoreTarget)
		{
			if (pointsModifier == 2 || !DartsGame.needDoubleToWin)
			{
				newTurn = false;
				activePlayer.Score += activePlayer.ScoreThisTurn;
				activePlayer.scoreHistory.Add(activePlayer.ScoreThisTurn);
				activePlayer.ScoreThisTurn = 0;
				Board.DartsDebug($"[DartsGameController] Player {activePlayer.UserID} has won the game with a score of {activePlayer.Score}!");
				StartPostGame();
				return;
			}
			Bust();
		}
		else if (activePlayer.Score + activePlayer.ScoreThisTurn == Board.scoreTarget - 1 && DartsGame.needDoubleToWin)
		{
			Bust();
		}
		if (newTurn)
		{
			activePlayer.Score += activePlayer.ScoreThisTurn;
			activePlayer.scoreHistory.Add(activePlayer.ScoreThisTurn);
			activePlayer.ScoreThisTurn = 0;
			activePlayer.DartsThrownThisTurn = 0;
			activePlayer.Turn++;
			Board.NewTurn();
		}
		Board.DartsDebug(string.Format("[DartsGameController] Turn [{0}] - Scored [{1}] points (base: {2}, modifier: {3}). Total score this turn: {4}, Total score: {5}, Darts thrown this turn: {6}, Total darts thrown: {7}, was a Bust: {8}", new object[9]
		{
			activePlayer.Turn,
			points * pointsModifier,
			points,
			pointsModifier,
			activePlayer.ScoreThisTurn,
			activePlayer.Score,
			activePlayer.DartsThrownThisTurn,
			activePlayer.DartsThrown,
			bustedTurn
		}));
		Board.SendNetworkUpdate();
		void Bust()
		{
			activePlayer.ScoreThisTurn = 0;
			newTurn = true;
			bustedTurn = true;
		}
	}

	public void ServerReceivedUpdatedTimer(BasePlayer player, float timeTaken)
	{
		if (IsInGame(player) && CanPlay(player) && IsAtBoard(player))
		{
			GetActivePlayerData().TimeTaken += timeTaken;
			Board.DartsDebug($"[DartsGameController] Updating server timer with time taken: {timeTaken}");
			Board.SendNetworkUpdate();
		}
	}
}
