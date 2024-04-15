namespace Chess.Game
{
	using System.Threading.Tasks;
	using System.Threading;
	using System.Collections.Generic;
	using UnityEngine;

	public class AIPlayer : Player
	{

		const int bookMoveDelayMillis = 500;

		Search search;
		AISettings settings;
		bool moveFound;
		Move move;
		Board board;
		CancellationTokenSource cancelSearchTimer;

		Book book;

		public AIPlayer(Board board, AISettings settings)
		{
			this.settings = settings;
			this.board = board;
			settings.requestAbortSearch += TimeOutThreadedSearch;
			settings.useFixedDepthSearch = false;
			// settings.depth = 20;
			// settings.useFixedDepthSearch = true;
			// settings.useIterativeDeepening = true;
			settings.searchTimeMillis = 1000 * 2;
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
			//prefer a random book move (opening)
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
				//when out of book (theory) then start using minimax 
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

		// Note: called outside of Unity main thread
		void TimeOutThreadedSearch()
		{
			Debug.Log(settings.searchTimeMillis + "ms search time reached, aborting search");
			if (cancelSearchTimer == null || !cancelSearchTimer.IsCancellationRequested)
			{
				search.EndSearch();
			}
		}
		void PlayBookMove(Move bookMove)
		{
			this.move = bookMove;
			moveFound = true;
		}
		void OnSearchComplete(Move move)
		{
			// Cancel search timer in case search finished before timer ran out (can happen when a mate is found)
			cancelSearchTimer?.Cancel();
			moveFound = true;
			this.move = move;
		}
	}
}