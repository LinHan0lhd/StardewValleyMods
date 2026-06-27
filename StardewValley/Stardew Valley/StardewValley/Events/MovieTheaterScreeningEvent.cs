using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley.Events
{
	// Token: 0x02000327 RID: 807
	public class MovieTheaterScreeningEvent
	{
		// Token: 0x0600349A RID: 13466 RVA: 0x0029DFBC File Offset: 0x0029C1BC
		public Event getMovieEvent(string movieId, List<List<Character>> player_and_guest_audience_groups, List<List<Character>> npcOnlyAudienceGroups, Dictionary<Character, MovieConcession> concessions_data = null)
		{
			this._concessionsData = concessions_data;
			this._responseOrder = new Dictionary<int, Character>();
			this._whiteListDependencyLookup = new Dictionary<Character, Character>();
			this._characterResponses = new Dictionary<Character, string>();
			this.movieData = MovieTheater.GetMovieDataById()[movieId];
			this.playerAndGuestAudienceGroups = player_and_guest_audience_groups;
			this.currentResponse = 0;
			StringBuilder sb = new StringBuilder();
			Random theaterRandom = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
			sb.Append("movieScreenAmbience/-2000 -2000/");
			string playerCharacterEventName = "farmer" + Utility.getFarmerNumberFromFarmer(Game1.player).ToString();
			string playerCharacterGuestName = "";
			bool hasPlayerGuest = false;
			foreach (List<Character> list in this.playerAndGuestAudienceGroups)
			{
				if (list.Contains(Game1.player))
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (!(list[i] is Farmer))
						{
							playerCharacterGuestName = list[i].name.Value;
							hasPlayerGuest = true;
							break;
						}
					}
				}
			}
			this._farmers = new List<Farmer>();
			foreach (List<Character> list2 in this.playerAndGuestAudienceGroups)
			{
				foreach (Character character3 in list2)
				{
					Farmer player = character3 as Farmer;
					if (player != null && !this._farmers.Contains(player))
					{
						this._farmers.Add(player);
					}
				}
			}
			List<Character> allAudience = this.playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).ToList<Character>();
			if (allAudience.Count <= 12)
			{
				allAudience.AddRange(npcOnlyAudienceGroups.SelectMany((List<Character> x) => x).ToList<Character>());
			}
			bool first = true;
			foreach (Character c in allAudience)
			{
				if (c != null)
				{
					if (!first)
					{
						sb.Append(' ');
					}
					Farmer f = c as Farmer;
					if (f != null)
					{
						sb.Append("farmer").Append(Utility.getFarmerNumberFromFarmer(f));
					}
					else
					{
						sb.Append(c.name.Value);
					}
					sb.Append(" -1000 -1000 0");
					first = false;
				}
			}
			sb.Append("/changeToTemporaryMap MovieTheaterScreen false/specificTemporarySprite movieTheater_setup/ambientLight 0 0 0/");
			string[] backRow = new string[8];
			string[] midRow = new string[6];
			string[] frontRow = new string[4];
			this.playerAndGuestAudienceGroups = (from x in this.playerAndGuestAudienceGroups
			orderby theaterRandom.Next()
			select x).ToList<List<Character>>();
			int startingSeat = theaterRandom.Next(8 - Math.Min(this.playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).Count<Character>(), 8) + 1);
			int whichGroup = 0;
			if (this.playerAndGuestAudienceGroups.Count > 0)
			{
				for (int j = 0; j < 8; j++)
				{
					int seat = (j + startingSeat) % 8;
					if (this.playerAndGuestAudienceGroups[whichGroup].Count == 2 && (seat == 3 || seat == 7))
					{
						j++;
						seat++;
						seat %= 8;
					}
					int k = 0;
					while (k < this.playerAndGuestAudienceGroups[whichGroup].Count && seat + k < backRow.Length)
					{
						backRow[seat + k] = ((this.playerAndGuestAudienceGroups[whichGroup][k] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(this.playerAndGuestAudienceGroups[whichGroup][k] as Farmer).ToString()) : this.playerAndGuestAudienceGroups[whichGroup][k].name.Value);
						if (k > 0)
						{
							j++;
						}
						k++;
					}
					whichGroup++;
					if (whichGroup >= this.playerAndGuestAudienceGroups.Count)
					{
						break;
					}
				}
			}
			else
			{
				Game1.log.Warn("The movie audience somehow has no players. This is likely a bug.");
			}
			bool usedMidRow = false;
			if (whichGroup < this.playerAndGuestAudienceGroups.Count)
			{
				startingSeat = 0;
				for (int l = 0; l < 4; l++)
				{
					int seat2 = (l + startingSeat) % 4;
					int m = 0;
					while (m < this.playerAndGuestAudienceGroups[whichGroup].Count && seat2 + m < frontRow.Length)
					{
						frontRow[seat2 + m] = ((this.playerAndGuestAudienceGroups[whichGroup][m] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(this.playerAndGuestAudienceGroups[whichGroup][m] as Farmer).ToString()) : this.playerAndGuestAudienceGroups[whichGroup][m].name.Value);
						if (m > 0)
						{
							l++;
						}
						m++;
					}
					whichGroup++;
					if (whichGroup >= this.playerAndGuestAudienceGroups.Count)
					{
						break;
					}
				}
				if (whichGroup < this.playerAndGuestAudienceGroups.Count)
				{
					usedMidRow = true;
					startingSeat = 0;
					for (int n = 0; n < 6; n++)
					{
						int seat3 = (n + startingSeat) % 6;
						if (this.playerAndGuestAudienceGroups[whichGroup].Count == 2 && seat3 == 2)
						{
							n++;
							seat3++;
							seat3 %= 8;
						}
						int j2 = 0;
						while (j2 < this.playerAndGuestAudienceGroups[whichGroup].Count && seat3 + j2 < midRow.Length)
						{
							midRow[seat3 + j2] = ((this.playerAndGuestAudienceGroups[whichGroup][j2] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(this.playerAndGuestAudienceGroups[whichGroup][j2] as Farmer).ToString()) : this.playerAndGuestAudienceGroups[whichGroup][j2].name.Value);
							if (j2 > 0)
							{
								n++;
							}
							j2++;
						}
						whichGroup++;
						if (whichGroup >= this.playerAndGuestAudienceGroups.Count)
						{
							break;
						}
					}
				}
			}
			if (!usedMidRow)
			{
				for (int j3 = 0; j3 < npcOnlyAudienceGroups.Count; j3++)
				{
					int seat4 = theaterRandom.Next(3 - npcOnlyAudienceGroups[j3].Count + 1) + j3 * 3;
					for (int i2 = 0; i2 < npcOnlyAudienceGroups[j3].Count; i2++)
					{
						midRow[seat4 + i2] = npcOnlyAudienceGroups[j3][i2].name.Value;
					}
				}
			}
			int soFar = 0;
			int sittingTogetherCount = 0;
			for (int i3 = 0; i3 < backRow.Length; i3++)
			{
				if (!string.IsNullOrEmpty(backRow[i3]) && backRow[i3] != playerCharacterEventName && backRow[i3] != playerCharacterGuestName)
				{
					soFar++;
					if (soFar >= 2)
					{
						sittingTogetherCount++;
						Point seat5 = this.getBackRowSeatTileFromIndex(i3);
						sb.Append("warp ").Append(backRow[i3]).Append(' ').Append(seat5.X).Append(' ').Append(seat5.Y).Append("/positionOffset ").Append(backRow[i3]).Append(" 0 -10/");
						if (sittingTogetherCount == 2)
						{
							sittingTogetherCount = 0;
							if (theaterRandom.NextBool() && backRow[i3] != playerCharacterGuestName && backRow[i3 - 1] != playerCharacterGuestName && backRow[i3 - 1] != null)
							{
								sb.Append("faceDirection ").Append(backRow[i3]).Append(" 3 true/");
								sb.Append("faceDirection ").Append(backRow[i3 - 1]).Append(" 1 true/");
							}
						}
					}
				}
			}
			soFar = 0;
			sittingTogetherCount = 0;
			for (int i4 = 0; i4 < midRow.Length; i4++)
			{
				if (!string.IsNullOrEmpty(midRow[i4]) && midRow[i4] != playerCharacterEventName && midRow[i4] != playerCharacterGuestName)
				{
					soFar++;
					if (soFar >= 2)
					{
						sittingTogetherCount++;
						Point seat6 = this.getMidRowSeatTileFromIndex(i4);
						sb.Append("warp ").Append(midRow[i4]).Append(' ').Append(seat6.X).Append(' ').Append(seat6.Y).Append("/positionOffset ").Append(midRow[i4]).Append(" 0 -10/");
						if (sittingTogetherCount == 2)
						{
							sittingTogetherCount = 0;
							if (i4 != 3 && theaterRandom.NextBool() && midRow[i4 - 1] != null)
							{
								sb.Append("faceDirection ").Append(midRow[i4]).Append(" 3 true/");
								sb.Append("faceDirection ").Append(midRow[i4 - 1]).Append(" 1 true/");
							}
						}
					}
				}
			}
			soFar = 0;
			sittingTogetherCount = 0;
			for (int i5 = 0; i5 < frontRow.Length; i5++)
			{
				if (!string.IsNullOrEmpty(frontRow[i5]) && frontRow[i5] != playerCharacterEventName && frontRow[i5] != playerCharacterGuestName)
				{
					soFar++;
					if (soFar >= 2)
					{
						sittingTogetherCount++;
						Point seat7 = this.getFrontRowSeatTileFromIndex(i5);
						sb.Append("warp ").Append(frontRow[i5]).Append(' ').Append(seat7.X).Append(' ').Append(seat7.Y).Append("/positionOffset ").Append(frontRow[i5]).Append(" 0 -10/");
						if (sittingTogetherCount == 2)
						{
							sittingTogetherCount = 0;
							if (theaterRandom.NextBool() && frontRow[i5 - 1] != null)
							{
								sb.Append("faceDirection ").Append(frontRow[i5]).Append(" 3 true/");
								sb.Append("faceDirection ").Append(frontRow[i5 - 1]).Append(" 1 true/");
							}
						}
					}
				}
			}
			Point warpPoint = new Point(1, 15);
			soFar = 0;
			for (int i6 = 0; i6 < backRow.Length; i6++)
			{
				if (!string.IsNullOrEmpty(backRow[i6]) && backRow[i6] != playerCharacterEventName && backRow[i6] != playerCharacterGuestName)
				{
					Point seat8 = this.getBackRowSeatTileFromIndex(i6);
					if (soFar == 1)
					{
						sb.Append("warp ").Append(backRow[i6]).Append(' ').Append(seat8.X - 1).Append(" 10").Append("/advancedMove ").Append(backRow[i6]).Append(" false 1 ").Append(200).Append(" 1 0 4 1000/").Append("positionOffset ").Append(backRow[i6]).Append(" 0 -10/");
					}
					else
					{
						sb.Append("warp ").Append(backRow[i6]).Append(" 1 12").Append("/advancedMove ").Append(backRow[i6]).Append(" false 1 200 ").Append("0 -2 ").Append(seat8.X - 1).Append(" 0 4 1000/").Append("positionOffset ").Append(backRow[i6]).Append(" 0 -10/");
					}
					soFar++;
				}
				if (soFar >= 2)
				{
					break;
				}
			}
			soFar = 0;
			for (int i7 = 0; i7 < midRow.Length; i7++)
			{
				if (!string.IsNullOrEmpty(midRow[i7]) && midRow[i7] != playerCharacterEventName && midRow[i7] != playerCharacterGuestName)
				{
					Point seat9 = this.getMidRowSeatTileFromIndex(i7);
					if (soFar == 1)
					{
						sb.Append("warp ").Append(midRow[i7]).Append(' ').Append(seat9.X - 1).Append(" 8").Append("/advancedMove ").Append(midRow[i7]).Append(" false 1 ").Append(400).Append(" 1 0 4 1000/");
					}
					else
					{
						sb.Append("warp ").Append(midRow[i7]).Append(" 2 9").Append("/advancedMove ").Append(midRow[i7]).Append(" false 1 300 ").Append("0 -1 ").Append(seat9.X - 2).Append(" 0 4 1000/");
					}
					soFar++;
				}
				if (soFar >= 2)
				{
					break;
				}
			}
			soFar = 0;
			for (int i8 = 0; i8 < frontRow.Length; i8++)
			{
				if (!string.IsNullOrEmpty(frontRow[i8]) && frontRow[i8] != playerCharacterEventName && frontRow[i8] != playerCharacterGuestName)
				{
					Point seat10 = this.getFrontRowSeatTileFromIndex(i8);
					if (soFar == 1)
					{
						sb.Append("warp ").Append(frontRow[i8]).Append(' ').Append(seat10.X - 1).Append(" 6").Append("/advancedMove ").Append(frontRow[i8]).Append(" false 1 ").Append(400).Append(" 1 0 4 1000/");
					}
					else
					{
						sb.Append("warp ").Append(frontRow[i8]).Append(" 3 7").Append("/advancedMove ").Append(frontRow[i8]).Append(" false 1 300 ").Append("0 -1 ").Append(seat10.X - 3).Append(" 0 4 1000/");
					}
					soFar++;
				}
				if (soFar >= 2)
				{
					break;
				}
			}
			sb.Append("viewport 6 8 true/pause 500/");
			for (int i9 = 0; i9 < backRow.Length; i9++)
			{
				if (!string.IsNullOrEmpty(backRow[i9]))
				{
					Point seat11 = this.getBackRowSeatTileFromIndex(i9);
					if (backRow[i9] == playerCharacterEventName || backRow[i9] == playerCharacterGuestName)
					{
						sb.Append("warp ").Append(backRow[i9]).Append(' ').Append(warpPoint.X).Append(' ').Append(warpPoint.Y).Append("/advancedMove ").Append(backRow[i9]).Append(" false 0 -5 ").Append(seat11.X - warpPoint.X).Append(" 0 4 1000/").Append("pause ").Append(1000).Append("/");
					}
				}
			}
			for (int i10 = 0; i10 < midRow.Length; i10++)
			{
				if (!string.IsNullOrEmpty(midRow[i10]))
				{
					Point seat12 = this.getMidRowSeatTileFromIndex(i10);
					if (midRow[i10] == playerCharacterEventName || midRow[i10] == playerCharacterGuestName)
					{
						sb.Append("warp ").Append(midRow[i10]).Append(' ').Append(warpPoint.X).Append(' ').Append(warpPoint.Y).Append("/advancedMove ").Append(midRow[i10]).Append(" false 0 -7 ").Append(seat12.X - warpPoint.X).Append(" 0 4 1000/").Append("pause ").Append(1000).Append("/");
					}
				}
			}
			for (int i11 = 0; i11 < frontRow.Length; i11++)
			{
				if (!string.IsNullOrEmpty(frontRow[i11]))
				{
					Point seat13 = this.getFrontRowSeatTileFromIndex(i11);
					if (frontRow[i11] == playerCharacterEventName || frontRow[i11] == playerCharacterGuestName)
					{
						sb.Append("warp ").Append(frontRow[i11]).Append(' ').Append(warpPoint.X).Append(' ').Append(warpPoint.Y).Append("/advancedMove ").Append(frontRow[i11]).Append(" false 0 -7 1 0 0 -1 1 0 0 -1 ").Append(seat13.X - 3).Append(" 0 4 1000/").Append("pause ").Append(1000).Append("/");
					}
				}
			}
			sb.Append("pause 3000");
			if (hasPlayerGuest)
			{
				sb.Append("/proceedPosition ").Append(playerCharacterGuestName);
			}
			sb.Append("/pause 1000");
			if (!hasPlayerGuest)
			{
				sb.Append("/proceedPosition farmer");
			}
			sb.Append("/waitForAllStationary/pause 100");
			foreach (Character c2 in allAudience)
			{
				string actorName = MovieTheaterScreeningEvent.getEventName(c2);
				if (actorName != playerCharacterEventName && actorName != playerCharacterGuestName)
				{
					if (c2 is Farmer)
					{
						sb.Append("/faceDirection ").Append(actorName).Append(" 0 true/positionOffset ").Append(actorName).Append(" 0 42 true");
					}
					else
					{
						sb.Append("/faceDirection ").Append(actorName).Append(" 0 true/positionOffset ").Append(actorName).Append(" 0 12 true");
					}
					if (theaterRandom.NextDouble() < 0.2)
					{
						sb.Append("/pause 100");
					}
				}
			}
			sb.Append("/positionOffset ").Append(playerCharacterEventName).Append(" 0 32");
			if (hasPlayerGuest)
			{
				sb.Append("/positionOffset ").Append(playerCharacterGuestName).Append(" 0 8");
			}
			sb.Append("/ambientLight 210 210 120 true/pause 500/viewport move 0 -1 4000/pause 5000");
			List<Character> responding_characters = new List<Character>();
			foreach (List<Character> list3 in this.playerAndGuestAudienceGroups)
			{
				foreach (Character character in list3)
				{
					if (!(character is Farmer) && !responding_characters.Contains(character))
					{
						responding_characters.Add(character);
					}
				}
			}
			for (int i12 = 0; i12 < responding_characters.Count; i12++)
			{
				int index = theaterRandom.Next(responding_characters.Count);
				Character character2 = responding_characters[i12];
				responding_characters[i12] = responding_characters[index];
				responding_characters[index] = character2;
			}
			int current_response_index = 0;
			foreach (MovieScene scene in this.movieData.Scenes)
			{
				if (scene.ResponsePoint != null)
				{
					bool found_reaction = false;
					for (int i13 = 0; i13 < responding_characters.Count; i13++)
					{
						MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_characters[i13] as NPC);
						if (reaction != null)
						{
							foreach (MovieReaction movie_reaction in reaction.Reactions)
							{
								if (movie_reaction.ShouldApplyToMovie(this.movieData, MovieTheater.GetPatronNames(), new string[]
								{
									MovieTheater.GetResponseForMovie(responding_characters[i13] as NPC)
								}))
								{
									SpecialResponses specialResponses = movie_reaction.SpecialResponses;
									if (((specialResponses != null) ? specialResponses.DuringMovie : null) != null && (movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint || movie_reaction.Whitelist.Count > 0))
									{
										if (!this._whiteListDependencyLookup.ContainsKey(responding_characters[i13]))
										{
											this._responseOrder[current_response_index] = responding_characters[i13];
											if (movie_reaction.Whitelist != null)
											{
												for (int j4 = 0; j4 < movie_reaction.Whitelist.Count; j4++)
												{
													Character white_list_character = Game1.getCharacterFromName(movie_reaction.Whitelist[j4], true, false);
													if (white_list_character != null)
													{
														this._whiteListDependencyLookup[white_list_character] = responding_characters[i13];
														foreach (int key in this._responseOrder.Keys)
														{
															if (this._responseOrder[key] == white_list_character)
															{
																this._responseOrder.Remove(key);
															}
														}
													}
												}
											}
										}
										responding_characters.RemoveAt(i13);
										i13--;
										found_reaction = true;
										break;
									}
								}
							}
							if (found_reaction)
							{
								break;
							}
						}
					}
					if (!found_reaction)
					{
						for (int i14 = 0; i14 < responding_characters.Count; i14++)
						{
							MovieCharacterReaction reaction2 = MovieTheater.GetReactionsForCharacter(responding_characters[i14] as NPC);
							if (reaction2 != null)
							{
								foreach (MovieReaction movie_reaction2 in reaction2.Reactions)
								{
									if (movie_reaction2.ShouldApplyToMovie(this.movieData, MovieTheater.GetPatronNames(), new string[]
									{
										MovieTheater.GetResponseForMovie(responding_characters[i14] as NPC)
									}))
									{
										SpecialResponses specialResponses2 = movie_reaction2.SpecialResponses;
										if (((specialResponses2 != null) ? specialResponses2.DuringMovie : null) != null && movie_reaction2.SpecialResponses.DuringMovie.ResponsePoint == current_response_index.ToString())
										{
											if (!this._whiteListDependencyLookup.ContainsKey(responding_characters[i14]))
											{
												this._responseOrder[current_response_index] = responding_characters[i14];
												if (movie_reaction2.Whitelist != null)
												{
													for (int j5 = 0; j5 < movie_reaction2.Whitelist.Count; j5++)
													{
														Character white_list_character2 = Game1.getCharacterFromName(movie_reaction2.Whitelist[j5], true, false);
														if (white_list_character2 != null)
														{
															this._whiteListDependencyLookup[white_list_character2] = responding_characters[i14];
															foreach (int key2 in this._responseOrder.Keys)
															{
																if (this._responseOrder[key2] == white_list_character2)
																{
																	this._responseOrder.Remove(key2);
																}
															}
														}
													}
												}
											}
											responding_characters.RemoveAt(i14);
											i14--;
											found_reaction = true;
											break;
										}
									}
								}
								if (found_reaction)
								{
									break;
								}
							}
						}
					}
					current_response_index++;
				}
			}
			current_response_index = 0;
			for (int i15 = 0; i15 < responding_characters.Count; i15++)
			{
				if (!this._whiteListDependencyLookup.ContainsKey(responding_characters[i15]))
				{
					while (this._responseOrder.ContainsKey(current_response_index))
					{
						current_response_index++;
					}
					this._responseOrder[current_response_index] = responding_characters[i15];
					current_response_index++;
				}
			}
			responding_characters = null;
			using (List<MovieScene>.Enumerator enumerator3 = this.movieData.Scenes.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					MovieScene scene2 = enumerator3.Current;
					this._ParseScene(sb, scene2);
				}
				goto IL_183B;
			}
			IL_1833:
			this._ParseResponse(sb, null);
			IL_183B:
			if (this.currentResponse >= this._responseOrder.Count)
			{
				sb.Append("/stopMusic");
				sb.Append("/fade/viewport -1000 -1000");
				sb.Append("/pause 500/message \"").Append(Game1.content.LoadString("Strings\\Locations:Theater_MovieEnd")).Append("\"/pause 500");
				sb.Append("/requestMovieEnd");
				return new Event(sb.ToString(), null, "MovieTheaterScreening", null);
			}
			goto IL_1833;
		}

		// Token: 0x0600349B RID: 13467 RVA: 0x0029F9B4 File Offset: 0x0029DBB4
		protected void _ParseScene(StringBuilder sb, MovieScene scene)
		{
			if (!string.IsNullOrWhiteSpace(scene.Sound))
			{
				sb.Append("/playSound ").Append(scene.Sound);
			}
			if (!string.IsNullOrWhiteSpace(scene.Music))
			{
				sb.Append("/playMusic ").Append(scene.Music);
			}
			if (scene.MessageDelay > 0)
			{
				sb.Append("/pause ").Append(scene.MessageDelay);
			}
			if (scene.Image >= 0)
			{
				sb.Append("/specificTemporarySprite movieTheater_screen ").Append(this.movieData.Id).Append(' ').Append(scene.Image).Append(' ').Append(scene.Shake);
				if (this.movieData.Texture != null)
				{
					sb.Append(" \"").Append(ArgUtility.EscapeQuotes(this.movieData.Texture)).Append('"');
				}
			}
			if (!string.IsNullOrWhiteSpace(scene.Script))
			{
				sb.Append(TokenParser.ParseText(scene.Script, null, null, null));
			}
			if (!string.IsNullOrWhiteSpace(scene.Text))
			{
				sb.Append("/message \"").Append(ArgUtility.EscapeQuotes(TokenParser.ParseText(scene.Text, null, null, null))).Append('"');
			}
			if (scene.ResponsePoint != null)
			{
				this._ParseResponse(sb, scene);
			}
		}

		// Token: 0x0600349C RID: 13468 RVA: 0x0029FB14 File Offset: 0x0029DD14
		protected void _ParseResponse(StringBuilder sb, MovieScene scene = null)
		{
			Character responding_character;
			if (this._responseOrder.TryGetValue(this.currentResponse, out responding_character))
			{
				sb.Append("/pause 500");
				bool hadUniqueScript = false;
				if (!this._whiteListDependencyLookup.ContainsKey(responding_character))
				{
					MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_character as NPC);
					if (reaction != null)
					{
						foreach (MovieReaction movie_reaction in reaction.Reactions)
						{
							if (movie_reaction.ShouldApplyToMovie(this.movieData, MovieTheater.GetPatronNames(), new string[]
							{
								MovieTheater.GetResponseForMovie(responding_character as NPC)
							}))
							{
								SpecialResponses specialResponses = movie_reaction.SpecialResponses;
								if (((specialResponses != null) ? specialResponses.DuringMovie : null) != null && (string.IsNullOrEmpty(movie_reaction.SpecialResponses.DuringMovie.ResponsePoint) || (scene != null && movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) || movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == this.currentResponse.ToString() || movie_reaction.Whitelist.Count > 0))
								{
									string script = TokenParser.ParseText(movie_reaction.SpecialResponses.DuringMovie.Script, null, null, null);
									string text = TokenParser.ParseText(movie_reaction.SpecialResponses.DuringMovie.Text, null, null, null);
									if (!string.IsNullOrWhiteSpace(script))
									{
										sb.Append(script);
										hadUniqueScript = true;
									}
									if (!string.IsNullOrWhiteSpace(text))
									{
										sb.Append("/speak ").Append(responding_character.name.Value).Append(" \"").Append(text).Append('"');
										break;
									}
									break;
								}
							}
						}
					}
				}
				this._ParseCharacterResponse(sb, responding_character, hadUniqueScript);
				foreach (Character key in this._whiteListDependencyLookup.Keys)
				{
					if (this._whiteListDependencyLookup[key] == responding_character)
					{
						this._ParseCharacterResponse(sb, key, false);
					}
				}
			}
			this.currentResponse++;
		}

		// Token: 0x0600349D RID: 13469 RVA: 0x0029FD78 File Offset: 0x0029DF78
		protected void _ParseCharacterResponse(StringBuilder sb, Character responding_character, bool ignoreScript = false)
		{
			string response = MovieTheater.GetResponseForMovie(responding_character as NPC);
			Character requestingCharacter;
			if (this._whiteListDependencyLookup.TryGetValue(responding_character, out requestingCharacter))
			{
				response = MovieTheater.GetResponseForMovie(requestingCharacter as NPC);
			}
			if (!(response == "love"))
			{
				if (!(response == "like"))
				{
					if (response == "dislike")
					{
						sb.Append("/friendship ").Append(responding_character.Name).Append(' ').Append(0);
						if (!ignoreScript)
						{
							sb.Append("/playSound newArtifact/emote ").Append(responding_character.name.Value).Append(' ').Append(24).Append("/message \"").Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeMovie", responding_character.displayName)).Append('"');
						}
					}
				}
				else
				{
					sb.Append("/friendship ").Append(responding_character.Name).Append(' ').Append(100);
					if (!ignoreScript)
					{
						sb.Append("/playSound give_gift/emote ").Append(responding_character.name.Value).Append(' ').Append(56).Append("/message \"").Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeMovie", responding_character.displayName)).Append('"');
					}
				}
			}
			else
			{
				sb.Append("/friendship ").Append(responding_character.Name).Append(' ').Append(200);
				if (!ignoreScript)
				{
					sb.Append("/playSound reward/emote ").Append(responding_character.name.Value).Append(' ').Append(20).Append("/message \"").Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveMovie", responding_character.displayName)).Append('"');
				}
			}
			MovieConcession concession;
			if (this._concessionsData != null && this._concessionsData.TryGetValue(responding_character, out concession))
			{
				string concession_response = MovieTheater.GetConcessionTasteForCharacter(responding_character, concession);
				string gender_tag = "";
				CharacterData npcData;
				if (NPC.TryGetData(responding_character.name.Value, out npcData))
				{
					Gender gender = npcData.Gender;
					if (gender != Gender.Male)
					{
						if (gender == Gender.Female)
						{
							gender_tag = "_Female";
						}
					}
					else
					{
						gender_tag = "_Male";
					}
				}
				string sound = "eat";
				if (concession.Tags != null && concession.Tags.Contains("Drink"))
				{
					sound = "gulp";
				}
				if (!(concession_response == "love"))
				{
					if (!(concession_response == "like"))
					{
						if (concession_response == "dislike")
						{
							sb.Append("/friendship ").Append(responding_character.Name).Append(' ').Append(0);
							sb.Append("/playSound croak/pause 1000");
							sb.Append("/playSound newArtifact/emote ").Append(responding_character.name.Value).Append(' ').Append(40).Append("/message \"").Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName)).Append('"');
						}
					}
					else
					{
						sb.Append("/friendship ").Append(responding_character.Name).Append(' ').Append(25);
						sb.Append("/tossConcession ").Append(responding_character.Name).Append(' ').Append(concession.Id).Append("/pause 1000");
						sb.Append("/playSound ").Append(sound).Append("/shake ").Append(responding_character.Name).Append(" 500/pause 1000");
						sb.Append("/playSound give_gift/emote ").Append(responding_character.name.Value).Append(' ').Append(56).Append("/message \"").Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName)).Append('"');
					}
				}
				else
				{
					sb.Append("/friendship ").Append(responding_character.Name).Append(' ').Append(50);
					sb.Append("/tossConcession ").Append(responding_character.Name).Append(' ').Append(concession.Id).Append("/pause 1000");
					sb.Append("/playSound ").Append(sound).Append("/shake ").Append(responding_character.Name).Append(" 500/pause 1000");
					sb.Append("/playSound reward/emote ").Append(responding_character.name.Value).Append(' ').Append(20).Append("/message \"").Append(Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveConcession" + gender_tag, responding_character.displayName, concession.DisplayName)).Append('"');
				}
			}
			this._characterResponses[responding_character] = response;
		}

		// Token: 0x0600349E RID: 13470 RVA: 0x002A02A3 File Offset: 0x0029E4A3
		public Dictionary<Character, string> GetCharacterResponses()
		{
			return this._characterResponses;
		}

		// Token: 0x0600349F RID: 13471 RVA: 0x002A02AC File Offset: 0x0029E4AC
		private static string getEventName(Character c)
		{
			Farmer player = c as Farmer;
			if (player != null)
			{
				return "farmer" + Utility.getFarmerNumberFromFarmer(player).ToString();
			}
			return c.name.Value;
		}

		// Token: 0x060034A0 RID: 13472 RVA: 0x002A02E8 File Offset: 0x0029E4E8
		private Point getBackRowSeatTileFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return new Point(2, 10);
			case 1:
				return new Point(3, 10);
			case 2:
				return new Point(4, 10);
			case 3:
				return new Point(5, 10);
			case 4:
				return new Point(8, 10);
			case 5:
				return new Point(9, 10);
			case 6:
				return new Point(10, 10);
			case 7:
				return new Point(11, 10);
			default:
				return new Point(4, 12);
			}
		}

		// Token: 0x060034A1 RID: 13473 RVA: 0x002A0370 File Offset: 0x0029E570
		private Point getMidRowSeatTileFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return new Point(3, 8);
			case 1:
				return new Point(4, 8);
			case 2:
				return new Point(5, 8);
			case 3:
				return new Point(8, 8);
			case 4:
				return new Point(9, 8);
			case 5:
				return new Point(10, 8);
			default:
				return new Point(4, 12);
			}
		}

		// Token: 0x060034A2 RID: 13474 RVA: 0x002A03D8 File Offset: 0x0029E5D8
		private Point getFrontRowSeatTileFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return new Point(4, 6);
			case 1:
				return new Point(5, 6);
			case 2:
				return new Point(8, 6);
			case 3:
				return new Point(9, 6);
			default:
				return new Point(4, 12);
			}
		}

		// Token: 0x04002250 RID: 8784
		public int currentResponse;

		// Token: 0x04002251 RID: 8785
		public List<List<Character>> playerAndGuestAudienceGroups;

		// Token: 0x04002252 RID: 8786
		public Dictionary<int, Character> _responseOrder = new Dictionary<int, Character>();

		// Token: 0x04002253 RID: 8787
		protected Dictionary<Character, Character> _whiteListDependencyLookup;

		// Token: 0x04002254 RID: 8788
		protected Dictionary<Character, string> _characterResponses;

		// Token: 0x04002255 RID: 8789
		public MovieData movieData;

		// Token: 0x04002256 RID: 8790
		protected List<Farmer> _farmers;

		// Token: 0x04002257 RID: 8791
		protected Dictionary<Character, MovieConcession> _concessionsData;
	}
}
