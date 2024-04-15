namespace Chess.Game
{
	using System.Threading.Tasks;
	using System.Threading;
	using System.Collections.Generic;
	using UnityEngine;

	public class AIPlayer2 : Player
	{

		const int bookMoveDelayMillis = 250;

		Search search;
		AISettings settings;
		bool moveFound;
		Move move;
		Board board;
		CancellationTokenSource cancelSearchTimer;

		Book book;

		public AIPlayer2(Board board, AISettings settings)
		{
			this.settings = settings;
			this.board = board;
			settings.requestAbortSearch += TimeOutThreadedSearch;
			settings.depth = 10;
			settings.useFixedDepthSearch = true;
			settings.useIterativeDeepening = true;
			settings.searchTimeMillis = 1000 * 1;
			search = new Search(board, settings);
			search.onSearchComplete += OnSearchComplete;
			search.searchDiagnostics = new Search.SearchDiagnostics();
			book = BookCreator.LoadBookFromFile(settings.book);
		}
		public override void Update()
		{
			if (moveFound)
			{
				moveFound = false;
				ChoseMove(move);
			}

			settings.diagnostics = search.searchDiagnostics;

		}

		public override void NotifyTurnToMove()
		{

			search.searchDiagnostics.isBook = false;
			moveFound = false;

			Move bookMove = Move.InvalidMove;
			if (settings.useBook && board.plyCount <= settings.maxBookPly)
			{
				if (book.HasPosition(board.ZobristKey))
				{
					bookMove = book.GetRandomBookMoveWeighted(board.ZobristKey);
				}
			}
			if (bookMove.IsInvalid)
			{
				if (settings.useThreading)
				{
					StartThreadedSearch();
				}
				else
				{
					StartSearch();
				}
			}
			else
			{

				search.searchDiagnostics.isBook = true;
				search.searchDiagnostics.moveVal = Chess.PGNCreator.NotationFromMove(FenUtility.CurrentFen(board), bookMove);
				settings.diagnostics = search.searchDiagnostics;
				Task.Delay(bookMoveDelayMillis).ContinueWith((t) => PlayBookMove(bookMove));

			}
		}

		void StartSearch()
		{
			search.StartSearch();
			moveFound = true;
		}
		void StartThreadedSearch()
		{
			//Thread thread = new Thread (new ThreadStart (search.StartSearch));
			//thread.Start ();
			Task.Factory.StartNew(() => search.StartSearch(), TaskCreationOptions.LongRunning);

			if (!settings.endlessSearchMode)
			{
				cancelSearchTimer = new CancellationTokenSource();
				Task.Delay(settings.searchTimeMillis, cancelSearchTimer.Token).ContinueWith((t) => TimeOutThreadedSearch());
			}

		}
		void TimeOutThreadedSearch()
		{
			if (cancelSearchTimer == null || !cancelSearchTimer.IsCancellationRequested)
			{
				Debug.Log(settings.searchTimeMillis + "ms search time reached, aborting search");
				search.EndSearch();
			}
			else
			{
				Debug.Log("Search time not reached");
			}
		}
		void PlayBookMove(Move bookMove)
		{
			this.move = bookMove;
			moveFound = true;
		}
		void OnSearchComplete(Move move)
		{
			cancelSearchTimer?.Cancel();
			moveFound = true;
			this.move = move;
		}
	}
}