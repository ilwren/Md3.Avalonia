using Md3.Avalonia.Icons;

namespace Md3.Avalonia.Controls;

/// <summary>Strongly named code points from the official Material Symbols Rounded catalog.</summary>
public static class MdSymbols
{
    private static string? Glyph(string value) => MdExternalMaterialSymbols.EnsureConfigured()
        ? MdExternalMaterialSymbols.ResolveGlyph(value)
        : null;

    private static readonly Lazy<IReadOnlyDictionary<string, string>> _nameToGlyph = new(() =>
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        dict["abc"] = "\ueb94";
        dict["ac_unit"] = "\ueb3b";
        dict["AcUnit"] = "\ueb3b";
        dict["access_alarm"] = "\ue855";
        dict["AccessAlarm"] = "\ue855";
        dict["access_alarms"] = "\ue855";
        dict["AccessAlarms"] = "\ue855";
        dict["access_time"] = "\uefd6";
        dict["AccessTime"] = "\uefd6";
        dict["access_time_filled"] = "\uefd6";
        dict["AccessTimeFilled"] = "\uefd6";
        dict["accessibility"] = "\ue84e";
        dict["accessibility_new"] = "\ue92c";
        dict["AccessibilityNew"] = "\ue92c";
        dict["accessible"] = "\ue914";
        dict["accessible_forward"] = "\ue934";
        dict["AccessibleForward"] = "\ue934";
        dict["accessible_menu"] = "\uf34e";
        dict["AccessibleMenu"] = "\uf34e";
        dict["account_balance"] = "\ue84f";
        dict["AccountBalance"] = "\ue84f";
        dict["account_balance_wallet"] = "\ue850";
        dict["AccountBalanceWallet"] = "\ue850";
        dict["account_box"] = "\ue851";
        dict["AccountBox"] = "\ue851";
        dict["account_child"] = "\ue852";
        dict["AccountChild"] = "\ue852";
        dict["account_child_invert"] = "\ue659";
        dict["AccountChildInvert"] = "\ue659";
        dict["account_circle"] = "\uf20b";
        dict["AccountCircle"] = "\uf20b";
        dict["account_circle_filled"] = "\uf20b";
        dict["AccountCircleFilled"] = "\uf20b";
        dict["account_circle_off"] = "\uf7b3";
        dict["AccountCircleOff"] = "\uf7b3";
        dict["account_tree"] = "\ue97a";
        dict["AccountTree"] = "\ue97a";
        dict["action_key"] = "\uf502";
        dict["ActionKey"] = "\uf502";
        dict["activity_zone"] = "\ue1e6";
        dict["ActivityZone"] = "\ue1e6";
        dict["acupuncture"] = "\uf2c4";
        dict["acute"] = "\ue4cb";
        dict["ad"] = "\ue65a";
        dict["ad_group"] = "\ue65b";
        dict["AdGroup"] = "\ue65b";
        dict["ad_group_off"] = "\ueae5";
        dict["AdGroupOff"] = "\ueae5";
        dict["ad_off"] = "\uf7b2";
        dict["AdOff"] = "\uf7b2";
        dict["ad_units"] = "\uf2eb";
        dict["AdUnits"] = "\uf2eb";
        dict["adaptive_audio_mic"] = "\uf4cc";
        dict["AdaptiveAudioMic"] = "\uf4cc";
        dict["adaptive_audio_mic_off"] = "\uf4cb";
        dict["AdaptiveAudioMicOff"] = "\uf4cb";
        dict["adb"] = "\ue60e";
        dict["add"] = "\ue145";
        dict["add_2"] = "\uf3dd";
        dict["Add2"] = "\uf3dd";
        dict["add_a_photo"] = "\ue439";
        dict["AddAPhoto"] = "\ue439";
        dict["add_ad"] = "\ue72a";
        dict["AddAd"] = "\ue72a";
        dict["add_alarm"] = "\ue856";
        dict["AddAlarm"] = "\ue856";
        dict["add_alert"] = "\ue003";
        dict["AddAlert"] = "\ue003";
        dict["add_box"] = "\ue146";
        dict["AddBox"] = "\ue146";
        dict["add_business"] = "\ue729";
        dict["AddBusiness"] = "\ue729";
        dict["add_call"] = "\uf0b7";
        dict["AddCall"] = "\uf0b7";
        dict["add_card"] = "\ueb86";
        dict["AddCard"] = "\ueb86";
        dict["add_chart"] = "\uef3c";
        dict["AddChart"] = "\uef3c";
        dict["add_circle"] = "\ue990";
        dict["AddCircle"] = "\ue990";
        dict["add_circle_outline"] = "\ue990";
        dict["AddCircleOutline"] = "\ue990";
        dict["add_column_left"] = "\uf425";
        dict["AddColumnLeft"] = "\uf425";
        dict["add_column_right"] = "\uf424";
        dict["AddColumnRight"] = "\uf424";
        dict["add_comment"] = "\ue266";
        dict["AddComment"] = "\ue266";
        dict["add_diamond"] = "\uf49c";
        dict["AddDiamond"] = "\uf49c";
        dict["add_home"] = "\uf8eb";
        dict["AddHome"] = "\uf8eb";
        dict["add_home_work"] = "\uf8ed";
        dict["AddHomeWork"] = "\uf8ed";
        dict["add_ic_call"] = "\uf0b7";
        dict["AddIcCall"] = "\uf0b7";
        dict["add_link"] = "\ue178";
        dict["AddLink"] = "\ue178";
        dict["add_location"] = "\ue567";
        dict["AddLocation"] = "\ue567";
        dict["add_location_alt"] = "\uef3a";
        dict["AddLocationAlt"] = "\uef3a";
        dict["add_moderator"] = "\ue97d";
        dict["AddModerator"] = "\ue97d";
        dict["add_notes"] = "\ue091";
        dict["AddNotes"] = "\ue091";
        dict["add_photo_alternate"] = "\ue43e";
        dict["AddPhotoAlternate"] = "\ue43e";
        dict["add_reaction"] = "\ue1d3";
        dict["AddReaction"] = "\ue1d3";
        dict["add_road"] = "\uef3b";
        dict["AddRoad"] = "\uef3b";
        dict["add_row_above"] = "\uf423";
        dict["AddRowAbove"] = "\uf423";
        dict["add_row_below"] = "\uf422";
        dict["AddRowBelow"] = "\uf422";
        dict["add_shopping_cart"] = "\ue854";
        dict["AddShoppingCart"] = "\ue854";
        dict["add_task"] = "\uf23a";
        dict["AddTask"] = "\uf23a";
        dict["add_to_drive"] = "\ue65c";
        dict["AddToDrive"] = "\ue65c";
        dict["add_to_home_screen"] = "\uf2b9";
        dict["AddToHomeScreen"] = "\uf2b9";
        dict["add_to_photos"] = "\ue39d";
        dict["AddToPhotos"] = "\ue39d";
        dict["add_to_queue"] = "\ue05c";
        dict["AddToQueue"] = "\ue05c";
        dict["add_triangle"] = "\uf48e";
        dict["AddTriangle"] = "\uf48e";
        dict["addchart"] = "\uef3c";
        dict["adf_scanner"] = "\ueada";
        dict["AdfScanner"] = "\ueada";
        dict["adjust"] = "\ue39e";
        dict["admin_meds"] = "\ue48d";
        dict["AdminMeds"] = "\ue48d";
        dict["admin_panel_settings"] = "\uef3d";
        dict["AdminPanelSettings"] = "\uef3d";
        dict["ads_click"] = "\ue762";
        dict["AdsClick"] = "\ue762";
        dict["agender"] = "\uf888";
        dict["agriculture"] = "\uea79";
        dict["air"] = "\uefd8";
        dict["air_freshener"] = "\ue2ca";
        dict["AirFreshener"] = "\ue2ca";
        dict["air_purifier"] = "\ue97e";
        dict["AirPurifier"] = "\ue97e";
        dict["air_purifier_gen"] = "\ue829";
        dict["AirPurifierGen"] = "\ue829";
        dict["airline_seat_flat"] = "\ue630";
        dict["AirlineSeatFlat"] = "\ue630";
        dict["airline_seat_flat_angled"] = "\ue631";
        dict["AirlineSeatFlatAngled"] = "\ue631";
        dict["airline_seat_individual_suite"] = "\ue632";
        dict["AirlineSeatIndividualSuite"] = "\ue632";
        dict["airline_seat_legroom_extra"] = "\ue633";
        dict["AirlineSeatLegroomExtra"] = "\ue633";
        dict["airline_seat_legroom_normal"] = "\ue634";
        dict["AirlineSeatLegroomNormal"] = "\ue634";
        dict["airline_seat_legroom_reduced"] = "\ue635";
        dict["AirlineSeatLegroomReduced"] = "\ue635";
        dict["airline_seat_recline_extra"] = "\ue636";
        dict["AirlineSeatReclineExtra"] = "\ue636";
        dict["airline_seat_recline_normal"] = "\ue637";
        dict["AirlineSeatReclineNormal"] = "\ue637";
        dict["airline_stops"] = "\ue7d0";
        dict["AirlineStops"] = "\ue7d0";
        dict["airlines"] = "\ue7ca";
        dict["airplane_ticket"] = "\uefd9";
        dict["AirplaneTicket"] = "\uefd9";
        dict["airplanemode_active"] = "\ue53d";
        dict["AirplanemodeActive"] = "\ue53d";
        dict["airplanemode_inactive"] = "\ue194";
        dict["AirplanemodeInactive"] = "\ue194";
        dict["airplay"] = "\ue055";
        dict["airport_shuttle"] = "\ueb3c";
        dict["AirportShuttle"] = "\ueb3c";
        dict["airware"] = "\uf154";
        dict["airwave"] = "\uf154";
        dict["alarm"] = "\ue855";
        dict["alarm_add"] = "\ue856";
        dict["AlarmAdd"] = "\ue856";
        dict["alarm_off"] = "\ue857";
        dict["AlarmOff"] = "\ue857";
        dict["alarm_on"] = "\ue858";
        dict["AlarmOn"] = "\ue858";
        dict["alarm_pause"] = "\uf35b";
        dict["AlarmPause"] = "\uf35b";
        dict["alarm_smart_wake"] = "\uf6b0";
        dict["AlarmSmartWake"] = "\uf6b0";
        dict["album"] = "\ue019";
        dict["align_center"] = "\ue356";
        dict["AlignCenter"] = "\ue356";
        dict["align_end"] = "\uf797";
        dict["AlignEnd"] = "\uf797";
        dict["align_flex_center"] = "\uf796";
        dict["AlignFlexCenter"] = "\uf796";
        dict["align_flex_end"] = "\uf795";
        dict["AlignFlexEnd"] = "\uf795";
        dict["align_flex_start"] = "\uf794";
        dict["AlignFlexStart"] = "\uf794";
        dict["align_horizontal_center"] = "\ue00f";
        dict["AlignHorizontalCenter"] = "\ue00f";
        dict["align_horizontal_left"] = "\ue00d";
        dict["AlignHorizontalLeft"] = "\ue00d";
        dict["align_horizontal_right"] = "\ue010";
        dict["AlignHorizontalRight"] = "\ue010";
        dict["align_items_stretch"] = "\uf793";
        dict["AlignItemsStretch"] = "\uf793";
        dict["align_justify_center"] = "\uf792";
        dict["AlignJustifyCenter"] = "\uf792";
        dict["align_justify_flex_end"] = "\uf791";
        dict["AlignJustifyFlexEnd"] = "\uf791";
        dict["align_justify_flex_start"] = "\uf790";
        dict["AlignJustifyFlexStart"] = "\uf790";
        dict["align_justify_space_around"] = "\uf78f";
        dict["AlignJustifySpaceAround"] = "\uf78f";
        dict["align_justify_space_between"] = "\uf78e";
        dict["AlignJustifySpaceBetween"] = "\uf78e";
        dict["align_justify_space_even"] = "\uf78d";
        dict["AlignJustifySpaceEven"] = "\uf78d";
        dict["align_justify_stretch"] = "\uf78c";
        dict["AlignJustifyStretch"] = "\uf78c";
        dict["align_self_stretch"] = "\uf78b";
        dict["AlignSelfStretch"] = "\uf78b";
        dict["align_space_around"] = "\uf78a";
        dict["AlignSpaceAround"] = "\uf78a";
        dict["align_space_between"] = "\uf789";
        dict["AlignSpaceBetween"] = "\uf789";
        dict["align_space_even"] = "\uf788";
        dict["AlignSpaceEven"] = "\uf788";
        dict["align_start"] = "\uf787";
        dict["AlignStart"] = "\uf787";
        dict["align_stretch"] = "\uf786";
        dict["AlignStretch"] = "\uf786";
        dict["align_vertical_bottom"] = "\ue015";
        dict["AlignVerticalBottom"] = "\ue015";
        dict["align_vertical_center"] = "\ue011";
        dict["AlignVerticalCenter"] = "\ue011";
        dict["align_vertical_top"] = "\ue00c";
        dict["AlignVerticalTop"] = "\ue00c";
        dict["all_inbox"] = "\ue97f";
        dict["AllInbox"] = "\ue97f";
        dict["all_inclusive"] = "\ueb3d";
        dict["AllInclusive"] = "\ueb3d";
        dict["all_match"] = "\ue093";
        dict["AllMatch"] = "\ue093";
        dict["all_out"] = "\ue90b";
        dict["AllOut"] = "\ue90b";
        dict["allergies"] = "\ue094";
        dict["allergy"] = "\ue64e";
        dict["alt_route"] = "\uf184";
        dict["AltRoute"] = "\uf184";
        dict["alternate_email"] = "\ue0e6";
        dict["AlternateEmail"] = "\ue0e6";
        dict["altitude"] = "\uf873";
        dict["ambient_screen"] = "\uf6c4";
        dict["AmbientScreen"] = "\uf6c4";
        dict["ambulance"] = "\uf803";
        dict["amend"] = "\uf802";
        dict["amp_stories"] = "\uea13";
        dict["AmpStories"] = "\uea13";
        dict["analytics"] = "\uef3e";
        dict["anchor"] = "\uf1cd";
        dict["android"] = "\ue859";
        dict["android_cell_4_bar"] = "\uef06";
        dict["AndroidCell4Bar"] = "\uef06";
        dict["android_cell_4_bar_alert"] = "\uef09";
        dict["AndroidCell4BarAlert"] = "\uef09";
        dict["android_cell_4_bar_off"] = "\uef08";
        dict["AndroidCell4BarOff"] = "\uef08";
        dict["android_cell_4_bar_plus"] = "\uef07";
        dict["AndroidCell4BarPlus"] = "\uef07";
        dict["android_cell_5_bar"] = "\uef02";
        dict["AndroidCell5Bar"] = "\uef02";
        dict["android_cell_5_bar_alert"] = "\uef05";
        dict["AndroidCell5BarAlert"] = "\uef05";
        dict["android_cell_5_bar_off"] = "\uef04";
        dict["AndroidCell5BarOff"] = "\uef04";
        dict["android_cell_5_bar_plus"] = "\uef03";
        dict["AndroidCell5BarPlus"] = "\uef03";
        dict["android_cell_dual_4_bar"] = "\uef0d";
        dict["AndroidCellDual4Bar"] = "\uef0d";
        dict["android_cell_dual_4_bar_alert"] = "\uef0f";
        dict["AndroidCellDual4BarAlert"] = "\uef0f";
        dict["android_cell_dual_4_bar_plus"] = "\uef0e";
        dict["AndroidCellDual4BarPlus"] = "\uef0e";
        dict["android_cell_dual_5_bar"] = "\uef0a";
        dict["AndroidCellDual5Bar"] = "\uef0a";
        dict["android_cell_dual_5_bar_alert"] = "\uef0c";
        dict["AndroidCellDual5BarAlert"] = "\uef0c";
        dict["android_cell_dual_5_bar_plus"] = "\uef0b";
        dict["AndroidCellDual5BarPlus"] = "\uef0b";
        dict["android_wifi_3_bar"] = "\uef16";
        dict["AndroidWifi3Bar"] = "\uef16";
        dict["android_wifi_3_bar_alert"] = "\uef1b";
        dict["AndroidWifi3BarAlert"] = "\uef1b";
        dict["android_wifi_3_bar_lock"] = "\uef1a";
        dict["AndroidWifi3BarLock"] = "\uef1a";
        dict["android_wifi_3_bar_off"] = "\uef19";
        dict["AndroidWifi3BarOff"] = "\uef19";
        dict["android_wifi_3_bar_plus"] = "\uef18";
        dict["AndroidWifi3BarPlus"] = "\uef18";
        dict["android_wifi_3_bar_question"] = "\uef17";
        dict["AndroidWifi3BarQuestion"] = "\uef17";
        dict["android_wifi_4_bar"] = "\uef10";
        dict["AndroidWifi4Bar"] = "\uef10";
        dict["android_wifi_4_bar_alert"] = "\uef15";
        dict["AndroidWifi4BarAlert"] = "\uef15";
        dict["android_wifi_4_bar_lock"] = "\uef14";
        dict["AndroidWifi4BarLock"] = "\uef14";
        dict["android_wifi_4_bar_off"] = "\uef13";
        dict["AndroidWifi4BarOff"] = "\uef13";
        dict["android_wifi_4_bar_plus"] = "\uef12";
        dict["AndroidWifi4BarPlus"] = "\uef12";
        dict["android_wifi_4_bar_question"] = "\uef11";
        dict["AndroidWifi4BarQuestion"] = "\uef11";
        dict["animated_images"] = "\uf49a";
        dict["AnimatedImages"] = "\uf49a";
        dict["animation"] = "\ue71c";
        dict["announcement"] = "\ue87f";
        dict["antigravity"] = "\U000FFFD2";
        dict["aod"] = "\uf2e6";
        dict["aod_tablet"] = "\uf89f";
        dict["AodTablet"] = "\uf89f";
        dict["aod_watch"] = "\uf6ac";
        dict["AodWatch"] = "\uf6ac";
        dict["apartment"] = "\uea40";
        dict["api"] = "\uf1b7";
        dict["apk_document"] = "\uf88e";
        dict["ApkDocument"] = "\uf88e";
        dict["apk_install"] = "\uf88f";
        dict["ApkInstall"] = "\uf88f";
        dict["app_badging"] = "\uf72f";
        dict["AppBadging"] = "\uf72f";
        dict["app_blocking"] = "\uf2e5";
        dict["AppBlocking"] = "\uf2e5";
        dict["app_promo"] = "\uf2cd";
        dict["AppPromo"] = "\uf2cd";
        dict["app_registration"] = "\uef40";
        dict["AppRegistration"] = "\uef40";
        dict["app_settings_alt"] = "\uf2d9";
        dict["AppSettingsAlt"] = "\uf2d9";
        dict["app_shortcut"] = "\uf2df";
        dict["AppShortcut"] = "\uf2df";
        dict["apparel"] = "\uef7b";
        dict["approval"] = "\ue982";
        dict["approval_delegation"] = "\uf84a";
        dict["ApprovalDelegation"] = "\uf84a";
        dict["approval_delegation_off"] = "\uf2c5";
        dict["ApprovalDelegationOff"] = "\uf2c5";
        dict["apps"] = "\ue5c3";
        dict["apps_outage"] = "\ue7cc";
        dict["AppsOutage"] = "\ue7cc";
        dict["aq"] = "\uf55a";
        dict["aq_indoor"] = "\uf55b";
        dict["AqIndoor"] = "\uf55b";
        dict["ar_on_you"] = "\uef7c";
        dict["ArOnYou"] = "\uef7c";
        dict["ar_stickers"] = "\ue983";
        dict["ArStickers"] = "\ue983";
        dict["architecture"] = "\uea3b";
        dict["archive"] = "\ue149";
        dict["area_chart"] = "\ue770";
        dict["AreaChart"] = "\ue770";
        dict["arming_countdown"] = "\ue78a";
        dict["ArmingCountdown"] = "\ue78a";
        dict["arrow_and_edge"] = "\uf5d7";
        dict["ArrowAndEdge"] = "\uf5d7";
        dict["arrow_back"] = "\ue5c4";
        dict["ArrowBack"] = "\ue5c4";
        dict["arrow_back_2"] = "\uf43a";
        dict["ArrowBack2"] = "\uf43a";
        dict["arrow_back_ios"] = "\ue5e0";
        dict["ArrowBackIos"] = "\ue5e0";
        dict["arrow_back_ios_new"] = "\ue2ea";
        dict["ArrowBackIosNew"] = "\ue2ea";
        dict["arrow_circle_down"] = "\uf181";
        dict["ArrowCircleDown"] = "\uf181";
        dict["arrow_circle_left"] = "\ueaa7";
        dict["ArrowCircleLeft"] = "\ueaa7";
        dict["arrow_circle_right"] = "\ueaaa";
        dict["ArrowCircleRight"] = "\ueaaa";
        dict["arrow_circle_up"] = "\uf182";
        dict["ArrowCircleUp"] = "\uf182";
        dict["arrow_cool_down"] = "\uf4b6";
        dict["ArrowCoolDown"] = "\uf4b6";
        dict["arrow_downward"] = "\ue5db";
        dict["ArrowDownward"] = "\ue5db";
        dict["arrow_downward_alt"] = "\ue984";
        dict["ArrowDownwardAlt"] = "\ue984";
        dict["arrow_drop_down"] = "\ue5c5";
        dict["ArrowDropDown"] = "\ue5c5";
        dict["arrow_drop_down_circle"] = "\ue5c6";
        dict["ArrowDropDownCircle"] = "\ue5c6";
        dict["arrow_drop_up"] = "\ue5c7";
        dict["ArrowDropUp"] = "\ue5c7";
        dict["arrow_forward"] = "\ue5c8";
        dict["ArrowForward"] = "\ue5c8";
        dict["arrow_forward_ios"] = "\ue5e1";
        dict["ArrowForwardIos"] = "\ue5e1";
        dict["arrow_insert"] = "\uf837";
        dict["ArrowInsert"] = "\uf837";
        dict["arrow_left"] = "\ue5de";
        dict["ArrowLeft"] = "\ue5de";
        dict["arrow_left_alt"] = "\uef7d";
        dict["ArrowLeftAlt"] = "\uef7d";
        dict["arrow_menu_close"] = "\uf3d3";
        dict["ArrowMenuClose"] = "\uf3d3";
        dict["arrow_menu_open"] = "\uf3d2";
        dict["ArrowMenuOpen"] = "\uf3d2";
        dict["arrow_or_edge"] = "\uf5d6";
        dict["ArrowOrEdge"] = "\uf5d6";
        dict["arrow_outward"] = "\uf8ce";
        dict["ArrowOutward"] = "\uf8ce";
        dict["arrow_range"] = "\uf69b";
        dict["ArrowRange"] = "\uf69b";
        dict["arrow_right"] = "\ue5df";
        dict["ArrowRight"] = "\ue5df";
        dict["arrow_right_alt"] = "\ue941";
        dict["ArrowRightAlt"] = "\ue941";
        dict["arrow_selector_tool"] = "\uf82f";
        dict["ArrowSelectorTool"] = "\uf82f";
        dict["arrow_shape_up"] = "\ueef6";
        dict["ArrowShapeUp"] = "\ueef6";
        dict["arrow_shape_up_stack"] = "\ueef7";
        dict["ArrowShapeUpStack"] = "\ueef7";
        dict["arrow_shape_up_stack_2"] = "\ueef8";
        dict["ArrowShapeUpStack2"] = "\ueef8";
        dict["arrow_split"] = "\uea04";
        dict["ArrowSplit"] = "\uea04";
        dict["arrow_top_left"] = "\uf72e";
        dict["ArrowTopLeft"] = "\uf72e";
        dict["arrow_top_right"] = "\uf72d";
        dict["ArrowTopRight"] = "\uf72d";
        dict["arrow_upload_progress"] = "\uf3f4";
        dict["ArrowUploadProgress"] = "\uf3f4";
        dict["arrow_upload_ready"] = "\uf3f5";
        dict["ArrowUploadReady"] = "\uf3f5";
        dict["arrow_upward"] = "\ue5d8";
        dict["ArrowUpward"] = "\ue5d8";
        dict["arrow_upward_alt"] = "\ue986";
        dict["ArrowUpwardAlt"] = "\ue986";
        dict["arrow_warm_up"] = "\uf4b5";
        dict["ArrowWarmUp"] = "\uf4b5";
        dict["arrows_input"] = "\uf394";
        dict["ArrowsInput"] = "\uf394";
        dict["arrows_left_right_circle"] = "\ueee4";
        dict["ArrowsLeftRightCircle"] = "\ueee4";
        dict["arrows_more_down"] = "\uf8ab";
        dict["ArrowsMoreDown"] = "\uf8ab";
        dict["arrows_more_up"] = "\uf8ac";
        dict["ArrowsMoreUp"] = "\uf8ac";
        dict["arrows_output"] = "\uf393";
        dict["ArrowsOutput"] = "\uf393";
        dict["arrows_outward"] = "\uf72c";
        dict["ArrowsOutward"] = "\uf72c";
        dict["arrows_up_down_circle"] = "\ueee3";
        dict["ArrowsUpDownCircle"] = "\ueee3";
        dict["art_track"] = "\ue060";
        dict["ArtTrack"] = "\ue060";
        dict["article"] = "\uef42";
        dict["article_person"] = "\uf368";
        dict["ArticlePerson"] = "\uf368";
        dict["article_shortcut"] = "\uf587";
        dict["ArticleShortcut"] = "\uf587";
        dict["artist"] = "\ue01a";
        dict["aspect_ratio"] = "\ue85b";
        dict["AspectRatio"] = "\ue85b";
        dict["assessment"] = "\uf0cc";
        dict["assignment"] = "\ue85d";
        dict["assignment_add"] = "\uf848";
        dict["AssignmentAdd"] = "\uf848";
        dict["assignment_globe"] = "\ueeec";
        dict["AssignmentGlobe"] = "\ueeec";
        dict["assignment_ind"] = "\ue85e";
        dict["AssignmentInd"] = "\ue85e";
        dict["assignment_late"] = "\ue85f";
        dict["AssignmentLate"] = "\ue85f";
        dict["assignment_return"] = "\ue860";
        dict["AssignmentReturn"] = "\ue860";
        dict["assignment_returned"] = "\ue861";
        dict["AssignmentReturned"] = "\ue861";
        dict["assignment_turned_in"] = "\ue862";
        dict["AssignmentTurnedIn"] = "\ue862";
        dict["assist_walker"] = "\uf8d5";
        dict["AssistWalker"] = "\uf8d5";
        dict["assistant"] = "\ue39f";
        dict["assistant_device"] = "\ue987";
        dict["AssistantDevice"] = "\ue987";
        dict["assistant_direction"] = "\ue988";
        dict["AssistantDirection"] = "\ue988";
        dict["assistant_navigation"] = "\ue989";
        dict["AssistantNavigation"] = "\ue989";
        dict["assistant_on_hub"] = "\uf6c1";
        dict["AssistantOnHub"] = "\uf6c1";
        dict["assistant_photo"] = "\uf0c6";
        dict["AssistantPhoto"] = "\uf0c6";
        dict["assured_workload"] = "\ueb6f";
        dict["AssuredWorkload"] = "\ueb6f";
        dict["asterisk"] = "\uf525";
        dict["astrophotography_auto"] = "\uf1d9";
        dict["AstrophotographyAuto"] = "\uf1d9";
        dict["astrophotography_off"] = "\uf1da";
        dict["AstrophotographyOff"] = "\uf1da";
        dict["atm"] = "\ue573";
        dict["atr"] = "\uebc7";
        dict["attach_email"] = "\uea5e";
        dict["AttachEmail"] = "\uea5e";
        dict["attach_file"] = "\ue226";
        dict["AttachFile"] = "\ue226";
        dict["attach_file_add"] = "\uf841";
        dict["AttachFileAdd"] = "\uf841";
        dict["attach_file_off"] = "\uf4d9";
        dict["AttachFileOff"] = "\uf4d9";
        dict["attach_money"] = "\ue227";
        dict["AttachMoney"] = "\ue227";
        dict["attachment"] = "\ue2bc";
        dict["attractions"] = "\uea52";
        dict["attribution"] = "\uefdb";
        dict["audio_capture"] = "\U000FFF03";
        dict["AudioCapture"] = "\U000FFF03";
        dict["audio_description"] = "\uf58c";
        dict["AudioDescription"] = "\uf58c";
        dict["audio_file"] = "\ueb82";
        dict["AudioFile"] = "\ueb82";
        dict["audio_video_receiver"] = "\uf5d3";
        dict["AudioVideoReceiver"] = "\uf5d3";
        dict["audiotrack"] = "\ue405";
        dict["auto_activity_zone"] = "\uf8ad";
        dict["AutoActivityZone"] = "\uf8ad";
        dict["auto_awesome"] = "\ue65f";
        dict["AutoAwesome"] = "\ue65f";
        dict["auto_awesome_mosaic"] = "\ue660";
        dict["AutoAwesomeMosaic"] = "\ue660";
        dict["auto_awesome_motion"] = "\ue661";
        dict["AutoAwesomeMotion"] = "\ue661";
        dict["auto_delete"] = "\uea4c";
        dict["AutoDelete"] = "\uea4c";
        dict["auto_detect_voice"] = "\uf83e";
        dict["AutoDetectVoice"] = "\uf83e";
        dict["auto_draw_solid"] = "\ue98a";
        dict["AutoDrawSolid"] = "\ue98a";
        dict["auto_fix"] = "\ue663";
        dict["AutoFix"] = "\ue663";
        dict["auto_fix_high"] = "\ue663";
        dict["AutoFixHigh"] = "\ue663";
        dict["auto_fix_normal"] = "\ue664";
        dict["AutoFixNormal"] = "\ue664";
        dict["auto_fix_off"] = "\ue665";
        dict["AutoFixOff"] = "\ue665";
        dict["auto_graph"] = "\ue4fb";
        dict["AutoGraph"] = "\ue4fb";
        dict["auto_label"] = "\uf6be";
        dict["AutoLabel"] = "\uf6be";
        dict["auto_meeting_room"] = "\uf6bf";
        dict["AutoMeetingRoom"] = "\uf6bf";
        dict["auto_mode"] = "\uec20";
        dict["AutoMode"] = "\uec20";
        dict["auto_read_pause"] = "\uf219";
        dict["AutoReadPause"] = "\uf219";
        dict["auto_read_play"] = "\uf216";
        dict["AutoReadPlay"] = "\uf216";
        dict["auto_schedule"] = "\ue214";
        dict["AutoSchedule"] = "\ue214";
        dict["auto_stories"] = "\ue666";
        dict["AutoStories"] = "\ue666";
        dict["auto_stories_off"] = "\uf267";
        dict["AutoStoriesOff"] = "\uf267";
        dict["auto_timer"] = "\uef7f";
        dict["AutoTimer"] = "\uef7f";
        dict["auto_towing"] = "\ue71e";
        dict["AutoTowing"] = "\ue71e";
        dict["auto_transmission"] = "\uf53f";
        dict["AutoTransmission"] = "\uf53f";
        dict["auto_videocam"] = "\uf6c0";
        dict["AutoVideocam"] = "\uf6c0";
        dict["autofps_select"] = "\uefdc";
        dict["AutofpsSelect"] = "\uefdc";
        dict["automation"] = "\uf421";
        dict["autopause"] = "\uf6b6";
        dict["autopay"] = "\uf84b";
        dict["autoplay"] = "\uf6b5";
        dict["autorenew"] = "\ue863";
        dict["autostop"] = "\uf682";
        dict["av1"] = "\uf4b0";
        dict["av_timer"] = "\ue01b";
        dict["AvTimer"] = "\ue01b";
        dict["avc"] = "\uf4af";
        dict["avg_pace"] = "\uf6bb";
        dict["AvgPace"] = "\uf6bb";
        dict["avg_time"] = "\uf813";
        dict["AvgTime"] = "\uf813";
        dict["avocado_bean"] = "\U000FFFA7";
        dict["AvocadoBean"] = "\U000FFFA7";
        dict["award_meal"] = "\uf241";
        dict["AwardMeal"] = "\uf241";
        dict["award_star"] = "\uf612";
        dict["AwardStar"] = "\uf612";
        dict["azm"] = "\uf6ec";
        dict["b_circle"] = "\ueee2";
        dict["BCircle"] = "\ueee2";
        dict["baby_changing_station"] = "\uf19b";
        dict["BabyChangingStation"] = "\uf19b";
        dict["back_hand"] = "\ue764";
        dict["BackHand"] = "\ue764";
        dict["back_to_tab"] = "\uf72b";
        dict["BackToTab"] = "\uf72b";
        dict["background_dot_large"] = "\uf79e";
        dict["BackgroundDotLarge"] = "\uf79e";
        dict["background_dot_small"] = "\uf514";
        dict["BackgroundDotSmall"] = "\uf514";
        dict["background_grid_small"] = "\uf79d";
        dict["BackgroundGridSmall"] = "\uf79d";
        dict["background_replace"] = "\uf20a";
        dict["BackgroundReplace"] = "\uf20a";
        dict["backlight_high"] = "\uf7ed";
        dict["BacklightHigh"] = "\uf7ed";
        dict["backlight_high_off"] = "\uf4ef";
        dict["BacklightHighOff"] = "\uf4ef";
        dict["backlight_low"] = "\uf7ec";
        dict["BacklightLow"] = "\uf7ec";
        dict["backpack"] = "\uf19c";
        dict["backspace"] = "\ue14a";
        dict["backup"] = "\ue864";
        dict["backup_table"] = "\uef43";
        dict["BackupTable"] = "\uef43";
        dict["badge"] = "\uea67";
        dict["badge_critical_battery"] = "\uf156";
        dict["BadgeCriticalBattery"] = "\uf156";
        dict["badminton"] = "\uf2a8";
        dict["bakery_dining"] = "\uea53";
        dict["BakeryDining"] = "\uea53";
        dict["balance"] = "\ueaf6";
        dict["balcony"] = "\ue58f";
        dict["ballot"] = "\ue172";
        dict["bar_chart"] = "\ue26b";
        dict["BarChart"] = "\ue26b";
        dict["bar_chart_4_bars"] = "\uf681";
        dict["BarChart4Bars"] = "\uf681";
        dict["bar_chart_off"] = "\uf411";
        dict["BarChartOff"] = "\uf411";
        dict["barcode"] = "\ue70b";
        dict["barcode_reader"] = "\uf85c";
        dict["BarcodeReader"] = "\uf85c";
        dict["barcode_scanner"] = "\ue70c";
        dict["BarcodeScanner"] = "\ue70c";
        dict["barefoot"] = "\uf871";
        dict["batch_prediction"] = "\uf0f5";
        dict["BatchPrediction"] = "\uf0f5";
        dict["bath_bedrock"] = "\uf286";
        dict["BathBedrock"] = "\uf286";
        dict["bath_outdoor"] = "\uf6fb";
        dict["BathOutdoor"] = "\uf6fb";
        dict["bath_private"] = "\uf6fa";
        dict["BathPrivate"] = "\uf6fa";
        dict["bath_public_large"] = "\uf6f9";
        dict["BathPublicLarge"] = "\uf6f9";
        dict["bath_soak"] = "\uf2a0";
        dict["BathSoak"] = "\uf2a0";
        dict["bathroom"] = "\uefdd";
        dict["bathtub"] = "\uea41";
        dict["battery_0_bar"] = "\uebdc";
        dict["Battery0Bar"] = "\uebdc";
        dict["battery_1_bar"] = "\uf09c";
        dict["Battery1Bar"] = "\uf09c";
        dict["battery_20"] = "\uf09c";
        dict["Battery20"] = "\uf09c";
        dict["battery_2_bar"] = "\uf09d";
        dict["Battery2Bar"] = "\uf09d";
        dict["battery_30"] = "\uf09d";
        dict["Battery30"] = "\uf09d";
        dict["battery_3_bar"] = "\uf09e";
        dict["Battery3Bar"] = "\uf09e";
        dict["battery_4_bar"] = "\uf09f";
        dict["Battery4Bar"] = "\uf09f";
        dict["battery_50"] = "\uf09e";
        dict["Battery50"] = "\uf09e";
        dict["battery_5_bar"] = "\uf0a0";
        dict["Battery5Bar"] = "\uf0a0";
        dict["battery_60"] = "\uf09f";
        dict["Battery60"] = "\uf09f";
        dict["battery_6_bar"] = "\uf0a1";
        dict["Battery6Bar"] = "\uf0a1";
        dict["battery_80"] = "\uf0a0";
        dict["Battery80"] = "\uf0a0";
        dict["battery_90"] = "\uf0a1";
        dict["Battery90"] = "\uf0a1";
        dict["battery_alert"] = "\ue19c";
        dict["BatteryAlert"] = "\ue19c";
        dict["battery_android_0"] = "\uf30d";
        dict["BatteryAndroid0"] = "\uf30d";
        dict["battery_android_1"] = "\uf30c";
        dict["BatteryAndroid1"] = "\uf30c";
        dict["battery_android_2"] = "\uf30b";
        dict["BatteryAndroid2"] = "\uf30b";
        dict["battery_android_3"] = "\uf30a";
        dict["BatteryAndroid3"] = "\uf30a";
        dict["battery_android_4"] = "\uf309";
        dict["BatteryAndroid4"] = "\uf309";
        dict["battery_android_5"] = "\uf308";
        dict["BatteryAndroid5"] = "\uf308";
        dict["battery_android_6"] = "\uf307";
        dict["BatteryAndroid6"] = "\uf307";
        dict["battery_android_alert"] = "\uf306";
        dict["BatteryAndroidAlert"] = "\uf306";
        dict["battery_android_bolt"] = "\uf305";
        dict["BatteryAndroidBolt"] = "\uf305";
        dict["battery_android_frame_1"] = "\uf257";
        dict["BatteryAndroidFrame1"] = "\uf257";
        dict["battery_android_frame_2"] = "\uf256";
        dict["BatteryAndroidFrame2"] = "\uf256";
        dict["battery_android_frame_3"] = "\uf255";
        dict["BatteryAndroidFrame3"] = "\uf255";
        dict["battery_android_frame_4"] = "\uf254";
        dict["BatteryAndroidFrame4"] = "\uf254";
        dict["battery_android_frame_5"] = "\uf253";
        dict["BatteryAndroidFrame5"] = "\uf253";
        dict["battery_android_frame_6"] = "\uf252";
        dict["BatteryAndroidFrame6"] = "\uf252";
        dict["battery_android_frame_alert"] = "\uf251";
        dict["BatteryAndroidFrameAlert"] = "\uf251";
        dict["battery_android_frame_bolt"] = "\uf250";
        dict["BatteryAndroidFrameBolt"] = "\uf250";
        dict["battery_android_frame_full"] = "\uf24f";
        dict["BatteryAndroidFrameFull"] = "\uf24f";
        dict["battery_android_frame_plus"] = "\uf24e";
        dict["BatteryAndroidFramePlus"] = "\uf24e";
        dict["battery_android_frame_question"] = "\uf24d";
        dict["BatteryAndroidFrameQuestion"] = "\uf24d";
        dict["battery_android_frame_share"] = "\uf24c";
        dict["BatteryAndroidFrameShare"] = "\uf24c";
        dict["battery_android_frame_shield"] = "\uf24b";
        dict["BatteryAndroidFrameShield"] = "\uf24b";
        dict["battery_android_full"] = "\uf304";
        dict["BatteryAndroidFull"] = "\uf304";
        dict["battery_android_plus"] = "\uf303";
        dict["BatteryAndroidPlus"] = "\uf303";
        dict["battery_android_question"] = "\uf302";
        dict["BatteryAndroidQuestion"] = "\uf302";
        dict["battery_android_share"] = "\uf301";
        dict["BatteryAndroidShare"] = "\uf301";
        dict["battery_android_shield"] = "\uf300";
        dict["BatteryAndroidShield"] = "\uf300";
        dict["battery_change"] = "\uf7eb";
        dict["BatteryChange"] = "\uf7eb";
        dict["battery_charging_20"] = "\uf0a2";
        dict["BatteryCharging20"] = "\uf0a2";
        dict["battery_charging_20_2"] = "\U000FFF3E";
        dict["BatteryCharging202"] = "\U000FFF3E";
        dict["battery_charging_30"] = "\uf0a3";
        dict["BatteryCharging30"] = "\uf0a3";
        dict["battery_charging_30_2"] = "\U000FFF3D";
        dict["BatteryCharging302"] = "\U000FFF3D";
        dict["battery_charging_50"] = "\uf0a4";
        dict["BatteryCharging50"] = "\uf0a4";
        dict["battery_charging_50_2"] = "\U000FFF3C";
        dict["BatteryCharging502"] = "\U000FFF3C";
        dict["battery_charging_60"] = "\uf0a5";
        dict["BatteryCharging60"] = "\uf0a5";
        dict["battery_charging_60_2"] = "\U000FFF3B";
        dict["BatteryCharging602"] = "\U000FFF3B";
        dict["battery_charging_80"] = "\uf0a6";
        dict["BatteryCharging80"] = "\uf0a6";
        dict["battery_charging_80_2"] = "\U000FFF3A";
        dict["BatteryCharging802"] = "\U000FFF3A";
        dict["battery_charging_90"] = "\uf0a7";
        dict["BatteryCharging90"] = "\uf0a7";
        dict["battery_charging_full"] = "\ue1a3";
        dict["BatteryChargingFull"] = "\ue1a3";
        dict["battery_charging_full_2"] = "\U000FFF39";
        dict["BatteryChargingFull2"] = "\U000FFF39";
        dict["battery_error"] = "\uf7ea";
        dict["BatteryError"] = "\uf7ea";
        dict["battery_full"] = "\ue1a5";
        dict["BatteryFull"] = "\ue1a5";
        dict["battery_full_alt"] = "\uf13b";
        dict["BatteryFullAlt"] = "\uf13b";
        dict["battery_horiz_000"] = "\uf8ae";
        dict["BatteryHoriz000"] = "\uf8ae";
        dict["battery_horiz_050"] = "\uf8af";
        dict["BatteryHoriz050"] = "\uf8af";
        dict["battery_horiz_075"] = "\uf8b0";
        dict["BatteryHoriz075"] = "\uf8b0";
        dict["battery_low"] = "\uf155";
        dict["BatteryLow"] = "\uf155";
        dict["battery_plus"] = "\uf7e9";
        dict["BatteryPlus"] = "\uf7e9";
        dict["battery_profile"] = "\ue206";
        dict["BatteryProfile"] = "\ue206";
        dict["battery_saver"] = "\uefde";
        dict["BatterySaver"] = "\uefde";
        dict["battery_share"] = "\uf67e";
        dict["BatteryShare"] = "\uf67e";
        dict["battery_status_good"] = "\uf67d";
        dict["BatteryStatusGood"] = "\uf67d";
        dict["battery_std"] = "\ue1a5";
        dict["BatteryStd"] = "\ue1a5";
        dict["battery_unknown"] = "\ue1a6";
        dict["BatteryUnknown"] = "\ue1a6";
        dict["battery_vert_005"] = "\uf8b1";
        dict["BatteryVert005"] = "\uf8b1";
        dict["battery_vert_020"] = "\uf8b2";
        dict["BatteryVert020"] = "\uf8b2";
        dict["battery_vert_050"] = "\uf8b3";
        dict["BatteryVert050"] = "\uf8b3";
        dict["battery_very_low"] = "\uf156";
        dict["BatteryVeryLow"] = "\uf156";
        dict["beach_access"] = "\ueb3e";
        dict["BeachAccess"] = "\ueb3e";
        dict["bed"] = "\uefdf";
        dict["bedroom_baby"] = "\uefe0";
        dict["BedroomBaby"] = "\uefe0";
        dict["bedroom_child"] = "\uefe1";
        dict["BedroomChild"] = "\uefe1";
        dict["bedroom_parent"] = "\uefe2";
        dict["BedroomParent"] = "\uefe2";
        dict["bedtime"] = "\uf159";
        dict["bedtime_off"] = "\ueb76";
        dict["BedtimeOff"] = "\ueb76";
        dict["beenhere"] = "\ue52d";
        dict["beer_meal"] = "\uf285";
        dict["BeerMeal"] = "\uf285";
        dict["bento"] = "\uf1f4";
        dict["bia"] = "\uf6eb";
        dict["bid_landscape"] = "\ue667";
        dict["BidLandscape"] = "\ue667";
        dict["bid_landscape_disabled"] = "\uef81";
        dict["BidLandscapeDisabled"] = "\uef81";
        dict["bigtop_updates"] = "\ue669";
        dict["BigtopUpdates"] = "\ue669";
        dict["bike_dock"] = "\uf47b";
        dict["BikeDock"] = "\uf47b";
        dict["bike_lane"] = "\uf47a";
        dict["BikeLane"] = "\uf47a";
        dict["bike_scooter"] = "\uef45";
        dict["BikeScooter"] = "\uef45";
        dict["biotech"] = "\uea3a";
        dict["blanket"] = "\ue828";
        dict["blender"] = "\uefe3";
        dict["blind"] = "\uf8d6";
        dict["blinds"] = "\ue286";
        dict["blinds_2"] = "\U000FFF78";
        dict["Blinds2"] = "\U000FFF78";
        dict["blinds_2_closed"] = "\U000FFF79";
        dict["Blinds2Closed"] = "\U000FFF79";
        dict["blinds_closed"] = "\uec1f";
        dict["BlindsClosed"] = "\uec1f";
        dict["block"] = "\uf08c";
        dict["blood_pressure"] = "\ue097";
        dict["BloodPressure"] = "\ue097";
        dict["bloodtype"] = "\uefe4";
        dict["bluetooth"] = "\ue1a7";
        dict["bluetooth_audio"] = "\ue60f";
        dict["BluetoothAudio"] = "\ue60f";
        dict["bluetooth_connected"] = "\ue1a8";
        dict["BluetoothConnected"] = "\ue1a8";
        dict["bluetooth_disabled"] = "\ue1a9";
        dict["BluetoothDisabled"] = "\ue1a9";
        dict["bluetooth_drive"] = "\uefe5";
        dict["BluetoothDrive"] = "\uefe5";
        dict["bluetooth_searching"] = "\ue60f";
        dict["BluetoothSearching"] = "\ue60f";
        dict["blur_circular"] = "\ue3a2";
        dict["BlurCircular"] = "\ue3a2";
        dict["blur_linear"] = "\ue3a3";
        dict["BlurLinear"] = "\ue3a3";
        dict["blur_medium"] = "\ue84c";
        dict["BlurMedium"] = "\ue84c";
        dict["blur_off"] = "\ue3a4";
        dict["BlurOff"] = "\ue3a4";
        dict["blur_on"] = "\ue3a5";
        dict["BlurOn"] = "\ue3a5";
        dict["blur_short"] = "\ue8cf";
        dict["BlurShort"] = "\ue8cf";
        dict["boat_bus"] = "\uf36d";
        dict["BoatBus"] = "\uf36d";
        dict["boat_railway"] = "\uf36c";
        dict["BoatRailway"] = "\uf36c";
        dict["body_fat"] = "\ue098";
        dict["BodyFat"] = "\ue098";
        dict["body_system"] = "\ue099";
        dict["BodySystem"] = "\ue099";
        dict["bolt"] = "\uea0b";
        dict["bolt_boost"] = "\U000FFF6A";
        dict["BoltBoost"] = "\U000FFF6A";
        dict["bomb"] = "\uf568";
        dict["book"] = "\ue86e";
        dict["book_2"] = "\uf53e";
        dict["Book2"] = "\uf53e";
        dict["book_3"] = "\uf53d";
        dict["Book3"] = "\uf53d";
        dict["book_4"] = "\uf53c";
        dict["Book4"] = "\uf53c";
        dict["book_5"] = "\uf53b";
        dict["Book5"] = "\uf53b";
        dict["book_6"] = "\uf3df";
        dict["Book6"] = "\uf3df";
        dict["book_online"] = "\uf2e4";
        dict["BookOnline"] = "\uf2e4";
        dict["book_ribbon"] = "\uf3e7";
        dict["BookRibbon"] = "\uf3e7";
        dict["bookmark"] = "\ue8e7";
        dict["bookmark_add"] = "\ue598";
        dict["BookmarkAdd"] = "\ue598";
        dict["bookmark_added"] = "\ue599";
        dict["BookmarkAdded"] = "\ue599";
        dict["bookmark_bag"] = "\uf410";
        dict["BookmarkBag"] = "\uf410";
        dict["bookmark_border"] = "\ue8e7";
        dict["BookmarkBorder"] = "\ue8e7";
        dict["bookmark_check"] = "\uf457";
        dict["BookmarkCheck"] = "\uf457";
        dict["bookmark_flag"] = "\uf456";
        dict["BookmarkFlag"] = "\uf456";
        dict["bookmark_heart"] = "\uf455";
        dict["BookmarkHeart"] = "\uf455";
        dict["bookmark_manager"] = "\uf7b1";
        dict["BookmarkManager"] = "\uf7b1";
        dict["bookmark_remove"] = "\ue59a";
        dict["BookmarkRemove"] = "\ue59a";
        dict["bookmark_stacks"] = "\ueee8";
        dict["BookmarkStacks"] = "\ueee8";
        dict["bookmark_star"] = "\uf454";
        dict["BookmarkStar"] = "\uf454";
        dict["bookmarks"] = "\ue98b";
        dict["books_movies_and_music"] = "\uef82";
        dict["BooksMoviesAndMusic"] = "\uef82";
        dict["border_all"] = "\ue228";
        dict["BorderAll"] = "\ue228";
        dict["border_bottom"] = "\ue229";
        dict["BorderBottom"] = "\ue229";
        dict["border_clear"] = "\ue22a";
        dict["BorderClear"] = "\ue22a";
        dict["border_color"] = "\ue22b";
        dict["BorderColor"] = "\ue22b";
        dict["border_horizontal"] = "\ue22c";
        dict["BorderHorizontal"] = "\ue22c";
        dict["border_inner"] = "\ue22d";
        dict["BorderInner"] = "\ue22d";
        dict["border_left"] = "\ue22e";
        dict["BorderLeft"] = "\ue22e";
        dict["border_outer"] = "\ue22f";
        dict["BorderOuter"] = "\ue22f";
        dict["border_right"] = "\ue230";
        dict["BorderRight"] = "\ue230";
        dict["border_style"] = "\ue231";
        dict["BorderStyle"] = "\ue231";
        dict["border_top"] = "\ue232";
        dict["BorderTop"] = "\ue232";
        dict["border_vertical"] = "\ue233";
        dict["BorderVertical"] = "\ue233";
        dict["borg"] = "\uf40d";
        dict["bottom_app_bar"] = "\ue730";
        dict["BottomAppBar"] = "\ue730";
        dict["bottom_drawer"] = "\ue72d";
        dict["BottomDrawer"] = "\ue72d";
        dict["bottom_navigation"] = "\ue98c";
        dict["BottomNavigation"] = "\ue98c";
        dict["bottom_panel_close"] = "\uf72a";
        dict["BottomPanelClose"] = "\uf72a";
        dict["bottom_panel_open"] = "\uf729";
        dict["BottomPanelOpen"] = "\uf729";
        dict["bottom_right_click"] = "\uf684";
        dict["BottomRightClick"] = "\uf684";
        dict["bottom_sheets"] = "\ue98d";
        dict["BottomSheets"] = "\ue98d";
        dict["box"] = "\uf5a4";
        dict["box_add"] = "\uf5a5";
        dict["BoxAdd"] = "\uf5a5";
        dict["box_edit"] = "\uf5a6";
        dict["BoxEdit"] = "\uf5a6";
        dict["boy"] = "\ueb67";
        dict["brand_awareness"] = "\ue98e";
        dict["BrandAwareness"] = "\ue98e";
        dict["brand_family"] = "\uf4f1";
        dict["BrandFamily"] = "\uf4f1";
        dict["branding_watermark"] = "\ue06b";
        dict["BrandingWatermark"] = "\ue06b";
        dict["breakfast_dining"] = "\uea54";
        dict["BreakfastDining"] = "\uea54";
        dict["breaking_news"] = "\uea08";
        dict["BreakingNews"] = "\uea08";
        dict["breaking_news_alt_1"] = "\uf0ba";
        dict["BreakingNewsAlt1"] = "\uf0ba";
        dict["breastfeeding"] = "\uf856";
        dict["brick"] = "\uf388";
        dict["briefcase_meal"] = "\uf246";
        dict["BriefcaseMeal"] = "\uf246";
        dict["brightness_1"] = "\ue3fa";
        dict["Brightness1"] = "\ue3fa";
        dict["brightness_2"] = "\uf036";
        dict["Brightness2"] = "\uf036";
        dict["brightness_3"] = "\ue3a8";
        dict["Brightness3"] = "\ue3a8";
        dict["brightness_4"] = "\ue3a9";
        dict["Brightness4"] = "\ue3a9";
        dict["brightness_5"] = "\ue3aa";
        dict["Brightness5"] = "\ue3aa";
        dict["brightness_6"] = "\ue3ab";
        dict["Brightness6"] = "\ue3ab";
        dict["brightness_7"] = "\ue3ac";
        dict["Brightness7"] = "\ue3ac";
        dict["brightness_alert"] = "\uf5cf";
        dict["BrightnessAlert"] = "\uf5cf";
        dict["brightness_auto"] = "\ue1ab";
        dict["BrightnessAuto"] = "\ue1ab";
        dict["brightness_empty"] = "\uf7e8";
        dict["BrightnessEmpty"] = "\uf7e8";
        dict["brightness_high"] = "\ue1ac";
        dict["BrightnessHigh"] = "\ue1ac";
        dict["brightness_low"] = "\ue1ad";
        dict["BrightnessLow"] = "\ue1ad";
        dict["brightness_medium"] = "\ue1ae";
        dict["BrightnessMedium"] = "\ue1ae";
        dict["bring_your_own_ip"] = "\ue016";
        dict["BringYourOwnIp"] = "\ue016";
        dict["broadcast_on_home"] = "\uf8f8";
        dict["BroadcastOnHome"] = "\uf8f8";
        dict["broadcast_on_personal"] = "\uf8f9";
        dict["BroadcastOnPersonal"] = "\uf8f9";
        dict["broken_image"] = "\ue3ad";
        dict["BrokenImage"] = "\ue3ad";
        dict["browse"] = "\ueb13";
        dict["browse_activity"] = "\uf8a5";
        dict["BrowseActivity"] = "\uf8a5";
        dict["browse_gallery"] = "\uebd1";
        dict["BrowseGallery"] = "\uebd1";
        dict["browser_not_supported"] = "\uef47";
        dict["BrowserNotSupported"] = "\uef47";
        dict["browser_updated"] = "\ue7cf";
        dict["BrowserUpdated"] = "\ue7cf";
        dict["brunch_dining"] = "\uea73";
        dict["BrunchDining"] = "\uea73";
        dict["brush"] = "\ue3ae";
        dict["bubble"] = "\uef83";
        dict["bubble_chart"] = "\ue6dd";
        dict["BubbleChart"] = "\ue6dd";
        dict["bubbles"] = "\uf64e";
        dict["bucket_check"] = "\uef2a";
        dict["BucketCheck"] = "\uef2a";
        dict["bug_report"] = "\ue868";
        dict["BugReport"] = "\ue868";
        dict["build"] = "\uf8cd";
        dict["build_circle"] = "\uef48";
        dict["BuildCircle"] = "\uef48";
        dict["bullet_chart"] = "\U000FFEC7";
        dict["BulletChart"] = "\U000FFEC7";
        dict["bungalow"] = "\ue591";
        dict["burst_mode"] = "\ue43c";
        dict["BurstMode"] = "\ue43c";
        dict["bus_alert"] = "\ue98f";
        dict["BusAlert"] = "\ue98f";
        dict["bus_map_pin"] = "\U000FFFA2";
        dict["BusMapPin"] = "\U000FFFA2";
        dict["bus_railway"] = "\uf36b";
        dict["BusRailway"] = "\uf36b";
        dict["business"] = "\ue7ee";
        dict["business_center"] = "\ueb3f";
        dict["BusinessCenter"] = "\ueb3f";
        dict["business_chip"] = "\uf84c";
        dict["BusinessChip"] = "\uf84c";
        dict["business_messages"] = "\uef84";
        dict["BusinessMessages"] = "\uef84";
        dict["buttons_alt"] = "\ue72f";
        dict["ButtonsAlt"] = "\ue72f";
        dict["cabin"] = "\ue589";
        dict["cable"] = "\uefe6";
        dict["cable_car"] = "\uf479";
        dict["CableCar"] = "\uf479";
        dict["cached"] = "\ue86a";
        dict["cadence"] = "\uf4b4";
        dict["cake"] = "\ue7e9";
        dict["cake_add"] = "\uf85b";
        dict["CakeAdd"] = "\uf85b";
        dict["calculate"] = "\uea5f";
        dict["calendar_add_on"] = "\uef85";
        dict["CalendarAddOn"] = "\uef85";
        dict["calendar_apps_script"] = "\uf0bb";
        dict["CalendarAppsScript"] = "\uf0bb";
        dict["calendar_check"] = "\uf243";
        dict["CalendarCheck"] = "\uf243";
        dict["calendar_clock"] = "\uf540";
        dict["CalendarClock"] = "\uf540";
        dict["calendar_lock"] = "\uf242";
        dict["CalendarLock"] = "\uf242";
        dict["calendar_meal"] = "\uf296";
        dict["CalendarMeal"] = "\uf296";
        dict["calendar_meal_2"] = "\uf240";
        dict["CalendarMeal2"] = "\uf240";
        dict["calendar_month"] = "\uebcc";
        dict["CalendarMonth"] = "\uebcc";
        dict["calendar_today"] = "\ue935";
        dict["CalendarToday"] = "\ue935";
        dict["calendar_view_day"] = "\ue936";
        dict["CalendarViewDay"] = "\ue936";
        dict["calendar_view_month"] = "\uefe7";
        dict["CalendarViewMonth"] = "\uefe7";
        dict["calendar_view_week"] = "\uefe8";
        dict["CalendarViewWeek"] = "\uefe8";
        dict["call"] = "\uf0d4";
        dict["call_end"] = "\uf0bc";
        dict["CallEnd"] = "\uf0bc";
        dict["call_end_alt"] = "\uf0bc";
        dict["CallEndAlt"] = "\uf0bc";
        dict["call_log"] = "\ue08e";
        dict["CallLog"] = "\ue08e";
        dict["call_made"] = "\ue0b2";
        dict["CallMade"] = "\ue0b2";
        dict["call_merge"] = "\ue0b3";
        dict["CallMerge"] = "\ue0b3";
        dict["call_missed"] = "\ue0b4";
        dict["CallMissed"] = "\ue0b4";
        dict["call_missed_outgoing"] = "\ue0e4";
        dict["CallMissedOutgoing"] = "\ue0e4";
        dict["call_quality"] = "\uf652";
        dict["CallQuality"] = "\uf652";
        dict["call_received"] = "\ue0b5";
        dict["CallReceived"] = "\ue0b5";
        dict["call_split"] = "\ue0b6";
        dict["CallSplit"] = "\ue0b6";
        dict["call_to_action"] = "\ue06c";
        dict["CallToAction"] = "\ue06c";
        dict["camera"] = "\ue3af";
        dict["camera_alt"] = "\ue412";
        dict["CameraAlt"] = "\ue412";
        dict["camera_enhance"] = "\ue8fc";
        dict["CameraEnhance"] = "\ue8fc";
        dict["camera_front"] = "\uf2c9";
        dict["CameraFront"] = "\uf2c9";
        dict["camera_indoor"] = "\uefe9";
        dict["CameraIndoor"] = "\uefe9";
        dict["camera_outdoor"] = "\uefea";
        dict["CameraOutdoor"] = "\uefea";
        dict["camera_rear"] = "\uf2c8";
        dict["CameraRear"] = "\uf2c8";
        dict["camera_roll"] = "\ue3b3";
        dict["CameraRoll"] = "\ue3b3";
        dict["camera_video"] = "\uf7a6";
        dict["CameraVideo"] = "\uf7a6";
        dict["cameraswitch"] = "\uefeb";
        dict["campaign"] = "\uef49";
        dict["camping"] = "\uf8a2";
        dict["cancel"] = "\ue888";
        dict["cancel_presentation"] = "\ue0e9";
        dict["CancelPresentation"] = "\ue0e9";
        dict["cancel_schedule_send"] = "\uea39";
        dict["CancelScheduleSend"] = "\uea39";
        dict["candle"] = "\uf588";
        dict["candlestick_chart"] = "\uead4";
        dict["CandlestickChart"] = "\uead4";
        dict["cannabis"] = "\uf2f3";
        dict["captive_portal"] = "\uf728";
        dict["CaptivePortal"] = "\uf728";
        dict["capture"] = "\uf727";
        dict["car_crash"] = "\uebf2";
        dict["CarCrash"] = "\uebf2";
        dict["car_defrost_left"] = "\uf344";
        dict["CarDefrostLeft"] = "\uf344";
        dict["car_defrost_low_left"] = "\uf343";
        dict["CarDefrostLowLeft"] = "\uf343";
        dict["car_defrost_low_right"] = "\uf342";
        dict["CarDefrostLowRight"] = "\uf342";
        dict["car_defrost_mid_left"] = "\uf278";
        dict["CarDefrostMidLeft"] = "\uf278";
        dict["car_defrost_mid_low_left"] = "\uf341";
        dict["CarDefrostMidLowLeft"] = "\uf341";
        dict["car_defrost_mid_low_right"] = "\uf277";
        dict["CarDefrostMidLowRight"] = "\uf277";
        dict["car_defrost_mid_right"] = "\uf340";
        dict["CarDefrostMidRight"] = "\uf340";
        dict["car_defrost_right"] = "\uf33f";
        dict["CarDefrostRight"] = "\uf33f";
        dict["car_fan_low_left"] = "\uf33e";
        dict["CarFanLowLeft"] = "\uf33e";
        dict["car_fan_low_mid_left"] = "\uf33d";
        dict["CarFanLowMidLeft"] = "\uf33d";
        dict["car_fan_low_right"] = "\uf33c";
        dict["CarFanLowRight"] = "\uf33c";
        dict["car_fan_mid_left"] = "\uf33b";
        dict["CarFanMidLeft"] = "\uf33b";
        dict["car_fan_mid_low_right"] = "\uf33a";
        dict["CarFanMidLowRight"] = "\uf33a";
        dict["car_fan_mid_right"] = "\uf339";
        dict["CarFanMidRight"] = "\uf339";
        dict["car_fan_recirculate"] = "\uf338";
        dict["CarFanRecirculate"] = "\uf338";
        dict["car_fan_recirculate_2"] = "\U000FFF40";
        dict["CarFanRecirculate2"] = "\U000FFF40";
        dict["car_gear"] = "\uf337";
        dict["CarGear"] = "\uf337";
        dict["car_lock"] = "\uf336";
        dict["CarLock"] = "\uf336";
        dict["car_mirror_heat"] = "\uf335";
        dict["CarMirrorHeat"] = "\uf335";
        dict["car_rental"] = "\uea55";
        dict["CarRental"] = "\uea55";
        dict["car_repair"] = "\uea56";
        dict["CarRepair"] = "\uea56";
        dict["car_seat_off"] = "\U000FFEBE";
        dict["CarSeatOff"] = "\U000FFEBE";
        dict["car_tag"] = "\uf4e3";
        dict["CarTag"] = "\uf4e3";
        dict["card_giftcard"] = "\ue8f6";
        dict["CardGiftcard"] = "\ue8f6";
        dict["card_membership"] = "\ue8f7";
        dict["CardMembership"] = "\ue8f7";
        dict["card_travel"] = "\ue8f8";
        dict["CardTravel"] = "\ue8f8";
        dict["cardio_load"] = "\uf4b9";
        dict["CardioLoad"] = "\uf4b9";
        dict["cardiology"] = "\ue09c";
        dict["cards"] = "\ue991";
        dict["cards_stack"] = "\uf38f";
        dict["CardsStack"] = "\uf38f";
        dict["cards_star"] = "\uf375";
        dict["CardsStar"] = "\uf375";
        dict["carpenter"] = "\uf1f8";
        dict["carry_on_bag"] = "\ueb08";
        dict["CarryOnBag"] = "\ueb08";
        dict["carry_on_bag_checked"] = "\ueb0b";
        dict["CarryOnBagChecked"] = "\ueb0b";
        dict["carry_on_bag_inactive"] = "\ueb0a";
        dict["CarryOnBagInactive"] = "\ueb0a";
        dict["carry_on_bag_question"] = "\ueb09";
        dict["CarryOnBagQuestion"] = "\ueb09";
        dict["cases"] = "\ue992";
        dict["casino"] = "\ueb40";
        dict["cast"] = "\ue307";
        dict["cast_connected"] = "\ue308";
        dict["CastConnected"] = "\ue308";
        dict["cast_for_education"] = "\uefec";
        dict["CastForEducation"] = "\uefec";
        dict["cast_pause"] = "\uf5f0";
        dict["CastPause"] = "\uf5f0";
        dict["cast_warning"] = "\uf5ef";
        dict["CastWarning"] = "\uf5ef";
        dict["castle"] = "\ueab1";
        dict["category"] = "\ue72c";
        dict["category_search"] = "\uf437";
        dict["CategorySearch"] = "\uf437";
        dict["celebration"] = "\uea65";
        dict["cell_merge"] = "\uf82e";
        dict["CellMerge"] = "\uf82e";
        dict["cell_tower"] = "\uebba";
        dict["CellTower"] = "\uebba";
        dict["cell_wifi"] = "\ue0ec";
        dict["CellWifi"] = "\ue0ec";
        dict["center_focus_strong"] = "\ue3b4";
        dict["CenterFocusStrong"] = "\ue3b4";
        dict["center_focus_weak"] = "\ue3b5";
        dict["CenterFocusWeak"] = "\ue3b5";
        dict["chair"] = "\uefed";
        dict["chair_alt"] = "\uefee";
        dict["ChairAlt"] = "\uefee";
        dict["chair_counter"] = "\uf29f";
        dict["ChairCounter"] = "\uf29f";
        dict["chair_fireplace"] = "\uf29e";
        dict["ChairFireplace"] = "\uf29e";
        dict["chair_umbrella"] = "\uf29d";
        dict["ChairUmbrella"] = "\uf29d";
        dict["chalet"] = "\ue585";
        dict["change_circle"] = "\ue2e7";
        dict["ChangeCircle"] = "\ue2e7";
        dict["change_history"] = "\ue86b";
        dict["ChangeHistory"] = "\ue86b";
        dict["charger"] = "\ue2ae";
        dict["charging_station"] = "\uf2e3";
        dict["ChargingStation"] = "\uf2e3";
        dict["chart_data"] = "\ue473";
        dict["ChartData"] = "\ue473";
        dict["chat"] = "\ue0c9";
        dict["chat_add_on"] = "\uf0f3";
        dict["ChatAddOn"] = "\uf0f3";
        dict["chat_apps_script"] = "\uf0bd";
        dict["ChatAppsScript"] = "\uf0bd";
        dict["chat_bubble"] = "\ue0cb";
        dict["ChatBubble"] = "\ue0cb";
        dict["chat_bubble_off"] = "\U000FFFBB";
        dict["ChatBubbleOff"] = "\U000FFFBB";
        dict["chat_bubble_outline"] = "\ue0cb";
        dict["ChatBubbleOutline"] = "\ue0cb";
        dict["chat_dashed"] = "\ueeed";
        dict["ChatDashed"] = "\ueeed";
        dict["chat_error"] = "\uf7ac";
        dict["ChatError"] = "\uf7ac";
        dict["chat_info"] = "\uf52b";
        dict["ChatInfo"] = "\uf52b";
        dict["chat_paste_go"] = "\uf6bd";
        dict["ChatPasteGo"] = "\uf6bd";
        dict["chat_paste_go_2"] = "\uf3cb";
        dict["ChatPasteGo2"] = "\uf3cb";
        dict["check"] = "\ue668";
        dict["check_alert"] = "\U000FFF85";
        dict["CheckAlert"] = "\U000FFF85";
        dict["check_box"] = "\ue9de";
        dict["CheckBox"] = "\ue9de";
        dict["check_box_outline_blank"] = "\ue835";
        dict["CheckBoxOutlineBlank"] = "\ue835";
        dict["check_circle"] = "\uf0be";
        dict["CheckCircle"] = "\uf0be";
        dict["check_circle_filled"] = "\uf0be";
        dict["CheckCircleFilled"] = "\uf0be";
        dict["check_circle_outline"] = "\uf0be";
        dict["CheckCircleOutline"] = "\uf0be";
        dict["check_circle_unread"] = "\uf27e";
        dict["CheckCircleUnread"] = "\uf27e";
        dict["check_in_out"] = "\uf6f6";
        dict["CheckInOut"] = "\uf6f6";
        dict["check_indeterminate_small"] = "\uf88a";
        dict["CheckIndeterminateSmall"] = "\uf88a";
        dict["check_small"] = "\uf88b";
        dict["CheckSmall"] = "\uf88b";
        dict["checkbook"] = "\ue70d";
        dict["checked_bag"] = "\ueb0c";
        dict["CheckedBag"] = "\ueb0c";
        dict["checked_bag_question"] = "\ueb0d";
        dict["CheckedBagQuestion"] = "\ueb0d";
        dict["checklist"] = "\ue6b1";
        dict["checklist_rtl"] = "\ue6b3";
        dict["ChecklistRtl"] = "\ue6b3";
        dict["checkroom"] = "\uf19e";
        dict["cheer"] = "\uf6a8";
        dict["chef_hat"] = "\uf357";
        dict["ChefHat"] = "\uf357";
        dict["chess"] = "\uf5e7";
        dict["chess_bishop"] = "\uf261";
        dict["ChessBishop"] = "\uf261";
        dict["chess_bishop_2"] = "\uf262";
        dict["ChessBishop2"] = "\uf262";
        dict["chess_king"] = "\uf25f";
        dict["ChessKing"] = "\uf25f";
        dict["chess_king_2"] = "\uf260";
        dict["ChessKing2"] = "\uf260";
        dict["chess_knight"] = "\uf25e";
        dict["ChessKnight"] = "\uf25e";
        dict["chess_pawn"] = "\uf3b6";
        dict["ChessPawn"] = "\uf3b6";
        dict["chess_pawn_2"] = "\uf25d";
        dict["ChessPawn2"] = "\uf25d";
        dict["chess_queen"] = "\uf25c";
        dict["ChessQueen"] = "\uf25c";
        dict["chess_rook"] = "\uf25b";
        dict["ChessRook"] = "\uf25b";
        dict["chevron_backward"] = "\uf46b";
        dict["ChevronBackward"] = "\uf46b";
        dict["chevron_forward"] = "\uf46a";
        dict["ChevronForward"] = "\uf46a";
        dict["chevron_left"] = "\ue5cb";
        dict["ChevronLeft"] = "\ue5cb";
        dict["chevron_line_up"] = "\ueec3";
        dict["ChevronLineUp"] = "\ueec3";
        dict["chevron_right"] = "\ue5cc";
        dict["ChevronRight"] = "\ue5cc";
        dict["child_care"] = "\ueb41";
        dict["ChildCare"] = "\ueb41";
        dict["child_friendly"] = "\uef80";
        dict["ChildFriendly"] = "\uef80";
        dict["child_hat"] = "\uef30";
        dict["ChildHat"] = "\uef30";
        dict["chip_extraction"] = "\uf821";
        dict["ChipExtraction"] = "\uf821";
        dict["chips"] = "\ue993";
        dict["chrome_reader_mode"] = "\ue86d";
        dict["ChromeReaderMode"] = "\ue86d";
        dict["chromecast_2"] = "\uf17b";
        dict["Chromecast2"] = "\uf17b";
        dict["chromecast_device"] = "\ue83c";
        dict["ChromecastDevice"] = "\ue83c";
        dict["chronic"] = "\uebb2";
        dict["church"] = "\ueaae";
        dict["cinematic_blur"] = "\uf853";
        dict["CinematicBlur"] = "\uf853";
        dict["circle"] = "\uef4a";
        dict["circle_circle"] = "\ueee1";
        dict["CircleCircle"] = "\ueee1";
        dict["circle_notifications"] = "\ue994";
        dict["CircleNotifications"] = "\ue994";
        dict["circles"] = "\ue7ea";
        dict["circles_ext"] = "\ue7ec";
        dict["CirclesExt"] = "\ue7ec";
        dict["clarify"] = "\uf0bf";
        dict["class"] = "\ue86e";
        dict["clean_hands"] = "\uf21f";
        dict["CleanHands"] = "\uf21f";
        dict["cleaning"] = "\ue995";
        dict["cleaning_bucket"] = "\uf8b4";
        dict["CleaningBucket"] = "\uf8b4";
        dict["cleaning_services"] = "\uf0ff";
        dict["CleaningServices"] = "\uf0ff";
        dict["clear"] = "\ue5cd";
        dict["clear_all"] = "\ue0b8";
        dict["ClearAll"] = "\ue0b8";
        dict["clear_day"] = "\uf157";
        dict["ClearDay"] = "\uf157";
        dict["clear_night"] = "\uf159";
        dict["ClearNight"] = "\uf159";
        dict["climate_mini_split"] = "\uf8b5";
        dict["ClimateMiniSplit"] = "\uf8b5";
        dict["clinical_notes"] = "\ue09e";
        dict["ClinicalNotes"] = "\ue09e";
        dict["clock_arrow_down"] = "\uf382";
        dict["ClockArrowDown"] = "\uf382";
        dict["clock_arrow_up"] = "\uf381";
        dict["ClockArrowUp"] = "\uf381";
        dict["clock_loader_10"] = "\uf726";
        dict["ClockLoader10"] = "\uf726";
        dict["clock_loader_20"] = "\uf725";
        dict["ClockLoader20"] = "\uf725";
        dict["clock_loader_40"] = "\uf724";
        dict["ClockLoader40"] = "\uf724";
        dict["clock_loader_60"] = "\uf723";
        dict["ClockLoader60"] = "\uf723";
        dict["clock_loader_80"] = "\uf722";
        dict["ClockLoader80"] = "\uf722";
        dict["clock_loader_90"] = "\uf721";
        dict["ClockLoader90"] = "\uf721";
        dict["close"] = "\ue5cd";
        dict["close_fullscreen"] = "\uf1cf";
        dict["CloseFullscreen"] = "\uf1cf";
        dict["close_small"] = "\uf508";
        dict["CloseSmall"] = "\uf508";
        dict["closed_caption"] = "\ue996";
        dict["ClosedCaption"] = "\ue996";
        dict["closed_caption_add"] = "\uf4ae";
        dict["ClosedCaptionAdd"] = "\uf4ae";
        dict["closed_caption_disabled"] = "\uf1dc";
        dict["ClosedCaptionDisabled"] = "\uf1dc";
        dict["closed_caption_off"] = "\ue996";
        dict["ClosedCaptionOff"] = "\ue996";
        dict["cloud"] = "\uf15c";
        dict["cloud_alert"] = "\uf3cc";
        dict["CloudAlert"] = "\uf3cc";
        dict["cloud_circle"] = "\ue2be";
        dict["CloudCircle"] = "\ue2be";
        dict["cloud_done"] = "\ue2bf";
        dict["CloudDone"] = "\ue2bf";
        dict["cloud_download"] = "\ue2c0";
        dict["CloudDownload"] = "\ue2c0";
        dict["cloud_lock"] = "\uf386";
        dict["CloudLock"] = "\uf386";
        dict["cloud_off"] = "\ue2c1";
        dict["CloudOff"] = "\ue2c1";
        dict["cloud_queue"] = "\uf15c";
        dict["CloudQueue"] = "\uf15c";
        dict["cloud_sync"] = "\ueb5a";
        dict["CloudSync"] = "\ueb5a";
        dict["cloud_upload"] = "\ue2c3";
        dict["CloudUpload"] = "\ue2c3";
        dict["cloudy"] = "\uf15c";
        dict["cloudy_filled"] = "\uf15c";
        dict["CloudyFilled"] = "\uf15c";
        dict["cloudy_snowing"] = "\ue810";
        dict["CloudySnowing"] = "\ue810";
        dict["co2"] = "\ue7b0";
        dict["co_present"] = "\ueaf0";
        dict["CoPresent"] = "\ueaf0";
        dict["code"] = "\ue86f";
        dict["code_blocks"] = "\uf84d";
        dict["CodeBlocks"] = "\uf84d";
        dict["code_off"] = "\ue4f3";
        dict["CodeOff"] = "\ue4f3";
        dict["code_xml"] = "\U000FFF8B";
        dict["CodeXml"] = "\U000FFF8B";
        dict["coffee"] = "\uefef";
        dict["coffee_maker"] = "\ueff0";
        dict["CoffeeMaker"] = "\ueff0";
        dict["cognition"] = "\ue09f";
        dict["cognition_2"] = "\uf3b5";
        dict["Cognition2"] = "\uf3b5";
        dict["collapse_all"] = "\ue944";
        dict["CollapseAll"] = "\ue944";
        dict["collapse_content"] = "\uf507";
        dict["CollapseContent"] = "\uf507";
        dict["collections"] = "\ue3d3";
        dict["collections_bookmark"] = "\ue431";
        dict["CollectionsBookmark"] = "\ue431";
        dict["color_lens"] = "\ue40a";
        dict["ColorLens"] = "\ue40a";
        dict["colorize"] = "\ue3b8";
        dict["colors"] = "\ue997";
        dict["combine_columns"] = "\uf420";
        dict["CombineColumns"] = "\uf420";
        dict["comedy_mask"] = "\uf4d6";
        dict["ComedyMask"] = "\uf4d6";
        dict["comic_bubble"] = "\uf5dd";
        dict["ComicBubble"] = "\uf5dd";
        dict["comment"] = "\ue24c";
        dict["comment_bank"] = "\uea4e";
        dict["CommentBank"] = "\uea4e";
        dict["comments_disabled"] = "\ue7a2";
        dict["CommentsDisabled"] = "\ue7a2";
        dict["commit"] = "\ueaf5";
        dict["communication"] = "\ue27c";
        dict["communities"] = "\ueb16";
        dict["communities_filled"] = "\ueb16";
        dict["CommunitiesFilled"] = "\ueb16";
        dict["commute"] = "\ue940";
        dict["compare"] = "\ue3b9";
        dict["compare_arrows"] = "\ue915";
        dict["CompareArrows"] = "\ue915";
        dict["compass_calibration"] = "\ue57c";
        dict["CompassCalibration"] = "\ue57c";
        dict["component_exchange"] = "\uf1e7";
        dict["ComponentExchange"] = "\uf1e7";
        dict["compost"] = "\ue761";
        dict["compress"] = "\ue94d";
        dict["computer"] = "\ue31e";
        dict["computer_arrow_up"] = "\uf2f7";
        dict["ComputerArrowUp"] = "\uf2f7";
        dict["computer_cancel"] = "\uf2f6";
        dict["ComputerCancel"] = "\uf2f6";
        dict["computer_sound"] = "\ueeb4";
        dict["ComputerSound"] = "\ueeb4";
        dict["concierge"] = "\uf561";
        dict["conditions"] = "\ue0a0";
        dict["confirmation_number"] = "\ue638";
        dict["ConfirmationNumber"] = "\ue638";
        dict["congenital"] = "\ue0a1";
        dict["connect_without_contact"] = "\uf223";
        dict["ConnectWithoutContact"] = "\uf223";
        dict["connected_tv"] = "\ue998";
        dict["ConnectedTv"] = "\ue998";
        dict["connecting_airports"] = "\ue7c9";
        dict["ConnectingAirports"] = "\ue7c9";
        dict["construction"] = "\uea3c";
        dict["contact_emergency"] = "\uf8d1";
        dict["ContactEmergency"] = "\uf8d1";
        dict["contact_mail"] = "\ue0d0";
        dict["ContactMail"] = "\ue0d0";
        dict["contact_page"] = "\uf22e";
        dict["ContactPage"] = "\uf22e";
        dict["contact_phone"] = "\uf0c0";
        dict["ContactPhone"] = "\uf0c0";
        dict["contact_phone_filled"] = "\uf0c0";
        dict["ContactPhoneFilled"] = "\uf0c0";
        dict["contact_support"] = "\ue94c";
        dict["ContactSupport"] = "\ue94c";
        dict["contactless"] = "\uea71";
        dict["contactless_off"] = "\uf858";
        dict["ContactlessOff"] = "\uf858";
        dict["contacts"] = "\ue0ba";
        dict["contacts_product"] = "\ue999";
        dict["ContactsProduct"] = "\ue999";
        dict["content_copy"] = "\ue14d";
        dict["ContentCopy"] = "\ue14d";
        dict["content_cut"] = "\ue14e";
        dict["ContentCut"] = "\ue14e";
        dict["content_paste"] = "\ue14f";
        dict["ContentPaste"] = "\ue14f";
        dict["content_paste_go"] = "\uea8e";
        dict["ContentPasteGo"] = "\uea8e";
        dict["content_paste_off"] = "\ue4f8";
        dict["ContentPasteOff"] = "\ue4f8";
        dict["content_paste_search"] = "\uea9b";
        dict["ContentPasteSearch"] = "\uea9b";
        dict["contextual_token"] = "\uf486";
        dict["ContextualToken"] = "\uf486";
        dict["contextual_token_add"] = "\uf485";
        dict["ContextualTokenAdd"] = "\uf485";
        dict["contract"] = "\uf5a0";
        dict["contract_delete"] = "\uf5a2";
        dict["ContractDelete"] = "\uf5a2";
        dict["contract_edit"] = "\uf5a1";
        dict["ContractEdit"] = "\uf5a1";
        dict["contrast"] = "\ueb37";
        dict["contrast_circle"] = "\uf49f";
        dict["ContrastCircle"] = "\uf49f";
        dict["contrast_rtl_off"] = "\uec72";
        dict["ContrastRtlOff"] = "\uec72";
        dict["contrast_square"] = "\uf4a0";
        dict["ContrastSquare"] = "\uf4a0";
        dict["control_camera"] = "\ue074";
        dict["ControlCamera"] = "\ue074";
        dict["control_point"] = "\ue990";
        dict["ControlPoint"] = "\ue990";
        dict["control_point_duplicate"] = "\ue3bb";
        dict["ControlPointDuplicate"] = "\ue3bb";
        dict["controller_gen"] = "\ue83d";
        dict["ControllerGen"] = "\ue83d";
        dict["conversation"] = "\uef2f";
        dict["conversion_path"] = "\uf0c1";
        dict["ConversionPath"] = "\uf0c1";
        dict["conversion_path_off"] = "\uf7b4";
        dict["ConversionPathOff"] = "\uf7b4";
        dict["convert_to_text"] = "\uf41f";
        dict["ConvertToText"] = "\uf41f";
        dict["conveyor_belt"] = "\uf867";
        dict["ConveyorBelt"] = "\uf867";
        dict["cookie"] = "\ueaac";
        dict["cookie_off"] = "\uf79a";
        dict["CookieOff"] = "\uf79a";
        dict["cooking"] = "\ue2b6";
        dict["cool_to_dry"] = "\ue276";
        dict["CoolToDry"] = "\ue276";
        dict["copy_all"] = "\ue2ec";
        dict["CopyAll"] = "\ue2ec";
        dict["copyright"] = "\ue90c";
        dict["coronavirus"] = "\uf221";
        dict["corporate_fare"] = "\uf1d0";
        dict["CorporateFare"] = "\uf1d0";
        dict["cottage"] = "\ue587";
        dict["counter_0"] = "\uf785";
        dict["Counter0"] = "\uf785";
        dict["counter_1"] = "\uf784";
        dict["Counter1"] = "\uf784";
        dict["counter_2"] = "\uf783";
        dict["Counter2"] = "\uf783";
        dict["counter_3"] = "\uf782";
        dict["Counter3"] = "\uf782";
        dict["counter_4"] = "\uf781";
        dict["Counter4"] = "\uf781";
        dict["counter_5"] = "\uf780";
        dict["Counter5"] = "\uf780";
        dict["counter_6"] = "\uf77f";
        dict["Counter6"] = "\uf77f";
        dict["counter_7"] = "\uf77e";
        dict["Counter7"] = "\uf77e";
        dict["counter_8"] = "\uf77d";
        dict["Counter8"] = "\uf77d";
        dict["counter_9"] = "\uf77c";
        dict["Counter9"] = "\uf77c";
        dict["countertops"] = "\uf1f7";
        dict["create"] = "\uf097";
        dict["create_new_folder"] = "\ue2cc";
        dict["CreateNewFolder"] = "\ue2cc";
        dict["credit_card"] = "\ue8a1";
        dict["CreditCard"] = "\ue8a1";
        dict["credit_card_clock"] = "\uf438";
        dict["CreditCardClock"] = "\uf438";
        dict["credit_card_gear"] = "\uf52d";
        dict["CreditCardGear"] = "\uf52d";
        dict["credit_card_heart"] = "\uf52c";
        dict["CreditCardHeart"] = "\uf52c";
        dict["credit_card_off"] = "\ue4f4";
        dict["CreditCardOff"] = "\ue4f4";
        dict["credit_score"] = "\ueff1";
        dict["CreditScore"] = "\ueff1";
        dict["crib"] = "\ue588";
        dict["crisis_alert"] = "\uebe9";
        dict["CrisisAlert"] = "\uebe9";
        dict["crop"] = "\ue3be";
        dict["crop_16_9"] = "\ue3bc";
        dict["Crop169"] = "\ue3bc";
        dict["crop_21_9"] = "\U000FFF0A";
        dict["Crop219"] = "\U000FFF0A";
        dict["crop_2_3"] = "\U000FFF0B";
        dict["Crop23"] = "\U000FFF0B";
        dict["crop_3_2"] = "\ue3bd";
        dict["Crop32"] = "\ue3bd";
        dict["crop_5_4"] = "\ue3bf";
        dict["Crop54"] = "\ue3bf";
        dict["crop_7_5"] = "\ue3c0";
        dict["Crop75"] = "\ue3c0";
        dict["crop_9_16"] = "\uf549";
        dict["Crop916"] = "\uf549";
        dict["crop_din"] = "\ue3c6";
        dict["CropDin"] = "\ue3c6";
        dict["crop_free"] = "\ue3c2";
        dict["CropFree"] = "\ue3c2";
        dict["crop_landscape"] = "\ue3c3";
        dict["CropLandscape"] = "\ue3c3";
        dict["crop_original"] = "\ue3f4";
        dict["CropOriginal"] = "\ue3f4";
        dict["crop_portrait"] = "\ue3c5";
        dict["CropPortrait"] = "\ue3c5";
        dict["crop_rotate"] = "\ue437";
        dict["CropRotate"] = "\ue437";
        dict["crop_square"] = "\ue3c6";
        dict["CropSquare"] = "\ue3c6";
        dict["crossword"] = "\uf5e5";
        dict["crowdsource"] = "\ueb18";
        dict["crown"] = "\uecb3";
        dict["cruelty_free"] = "\ue799";
        dict["CrueltyFree"] = "\ue799";
        dict["css"] = "\ueb93";
        dict["csv"] = "\ue6cf";
        dict["currency_bitcoin"] = "\uebc5";
        dict["CurrencyBitcoin"] = "\uebc5";
        dict["currency_exchange"] = "\ueb70";
        dict["CurrencyExchange"] = "\ueb70";
        dict["currency_franc"] = "\ueafa";
        dict["CurrencyFranc"] = "\ueafa";
        dict["currency_lira"] = "\ueaef";
        dict["CurrencyLira"] = "\ueaef";
        dict["currency_pound"] = "\ueaf1";
        dict["CurrencyPound"] = "\ueaf1";
        dict["currency_ruble"] = "\ueaec";
        dict["CurrencyRuble"] = "\ueaec";
        dict["currency_rupee"] = "\ueaf7";
        dict["CurrencyRupee"] = "\ueaf7";
        dict["currency_rupee_circle"] = "\uf460";
        dict["CurrencyRupeeCircle"] = "\uf460";
        dict["currency_yen"] = "\ueafb";
        dict["CurrencyYen"] = "\ueafb";
        dict["currency_yuan"] = "\ueaf9";
        dict["CurrencyYuan"] = "\ueaf9";
        dict["curtains"] = "\uec1e";
        dict["curtains_closed"] = "\uec1d";
        dict["CurtainsClosed"] = "\uec1d";
        dict["custom_typography"] = "\ue732";
        dict["CustomTypography"] = "\ue732";
        dict["cut"] = "\uf08b";
        dict["cycle"] = "\uf854";
        dict["cyclone"] = "\uebd5";
        dict["dangerous"] = "\ue99a";
        dict["dark_mode"] = "\ue51c";
        dict["DarkMode"] = "\ue51c";
        dict["dashboard"] = "\ue871";
        dict["dashboard_2"] = "\uf3ea";
        dict["Dashboard2"] = "\uf3ea";
        dict["dashboard_2_add"] = "\U000FFEE9";
        dict["Dashboard2Add"] = "\U000FFEE9";
        dict["dashboard_2_edit"] = "\U000FFFD7";
        dict["Dashboard2Edit"] = "\U000FFFD7";
        dict["dashboard_2_gear"] = "\U000FFFD6";
        dict["Dashboard2Gear"] = "\U000FFFD6";
        dict["dashboard_customize"] = "\ue99b";
        dict["DashboardCustomize"] = "\ue99b";
        dict["data_alert"] = "\uf7f6";
        dict["DataAlert"] = "\uf7f6";
        dict["data_array"] = "\uead1";
        dict["DataArray"] = "\uead1";
        dict["data_check"] = "\uf7f2";
        dict["DataCheck"] = "\uf7f2";
        dict["data_exploration"] = "\ue76f";
        dict["DataExploration"] = "\ue76f";
        dict["data_info_alert"] = "\uf7f5";
        dict["DataInfoAlert"] = "\uf7f5";
        dict["data_loss_prevention"] = "\ue2dc";
        dict["DataLossPrevention"] = "\ue2dc";
        dict["data_object"] = "\uead3";
        dict["DataObject"] = "\uead3";
        dict["data_saver_off"] = "\ueff2";
        dict["DataSaverOff"] = "\ueff2";
        dict["data_saver_on"] = "\ueff3";
        dict["DataSaverOn"] = "\ueff3";
        dict["data_table"] = "\ue99c";
        dict["DataTable"] = "\ue99c";
        dict["data_thresholding"] = "\ueb9f";
        dict["DataThresholding"] = "\ueb9f";
        dict["data_usage"] = "\ueff2";
        dict["DataUsage"] = "\ueff2";
        dict["database"] = "\uf20e";
        dict["database_off"] = "\uf414";
        dict["DatabaseOff"] = "\uf414";
        dict["database_search"] = "\uf38e";
        dict["DatabaseSearch"] = "\uf38e";
        dict["database_upload"] = "\uf3dc";
        dict["DatabaseUpload"] = "\uf3dc";
        dict["dataset"] = "\uf8ee";
        dict["dataset_linked"] = "\uf8ef";
        dict["DatasetLinked"] = "\uf8ef";
        dict["date_range"] = "\ue916";
        dict["DateRange"] = "\ue916";
        dict["deblur"] = "\ueb77";
        dict["deceased"] = "\ue0a5";
        dict["decimal_decrease"] = "\uf82d";
        dict["DecimalDecrease"] = "\uf82d";
        dict["decimal_increase"] = "\uf82c";
        dict["DecimalIncrease"] = "\uf82c";
        dict["deck"] = "\uea42";
        dict["dehaze"] = "\ue3c7";
        dict["delete"] = "\ue92e";
        dict["delete_forever"] = "\ue92b";
        dict["DeleteForever"] = "\ue92b";
        dict["delete_history"] = "\uf518";
        dict["DeleteHistory"] = "\uf518";
        dict["delete_outline"] = "\ue92e";
        dict["DeleteOutline"] = "\ue92e";
        dict["delete_sweep"] = "\ue16c";
        dict["DeleteSweep"] = "\ue16c";
        dict["delivery_dining"] = "\ueb28";
        dict["DeliveryDining"] = "\ueb28";
        dict["delivery_truck_bolt"] = "\uf3a2";
        dict["DeliveryTruckBolt"] = "\uf3a2";
        dict["delivery_truck_speed"] = "\uf3a1";
        dict["DeliveryTruckSpeed"] = "\uf3a1";
        dict["demography"] = "\ue489";
        dict["density_large"] = "\ueba9";
        dict["DensityLarge"] = "\ueba9";
        dict["density_medium"] = "\ueb9e";
        dict["DensityMedium"] = "\ueb9e";
        dict["density_small"] = "\ueba8";
        dict["DensitySmall"] = "\ueba8";
        dict["dentistry"] = "\ue0a6";
        dict["departure_board"] = "\ue576";
        dict["DepartureBoard"] = "\ue576";
        dict["deployed_code"] = "\uf720";
        dict["DeployedCode"] = "\uf720";
        dict["deployed_code_account"] = "\uf51b";
        dict["DeployedCodeAccount"] = "\uf51b";
        dict["deployed_code_alert"] = "\uf5f2";
        dict["DeployedCodeAlert"] = "\uf5f2";
        dict["deployed_code_history"] = "\uf5f3";
        dict["DeployedCodeHistory"] = "\uf5f3";
        dict["deployed_code_update"] = "\uf5f4";
        dict["DeployedCodeUpdate"] = "\uf5f4";
        dict["dermatology"] = "\ue0a7";
        dict["description"] = "\ue873";
        dict["deselect"] = "\uebb6";
        dict["design_services"] = "\uf10a";
        dict["DesignServices"] = "\uf10a";
        dict["desk"] = "\uf8f4";
        dict["deskphone"] = "\uf7fa";
        dict["desktop_access_disabled"] = "\ue99d";
        dict["DesktopAccessDisabled"] = "\ue99d";
        dict["desktop_cloud"] = "\uf3db";
        dict["DesktopCloud"] = "\uf3db";
        dict["desktop_cloud_stack"] = "\uf3be";
        dict["DesktopCloudStack"] = "\uf3be";
        dict["desktop_landscape"] = "\uf45e";
        dict["DesktopLandscape"] = "\uf45e";
        dict["desktop_landscape_add"] = "\uf439";
        dict["DesktopLandscapeAdd"] = "\uf439";
        dict["desktop_mac"] = "\ue30b";
        dict["DesktopMac"] = "\ue30b";
        dict["desktop_portrait"] = "\uf45d";
        dict["DesktopPortrait"] = "\uf45d";
        dict["desktop_windows"] = "\ue30c";
        dict["DesktopWindows"] = "\ue30c";
        dict["destruction"] = "\uf585";
        dict["details"] = "\ue3c8";
        dict["detection_and_zone"] = "\ue29f";
        dict["DetectionAndZone"] = "\ue29f";
        dict["detection_and_zone_off"] = "\ueebf";
        dict["DetectionAndZoneOff"] = "\ueebf";
        dict["detector"] = "\ue282";
        dict["detector_alarm"] = "\ue1f7";
        dict["DetectorAlarm"] = "\ue1f7";
        dict["detector_battery"] = "\ue204";
        dict["DetectorBattery"] = "\ue204";
        dict["detector_co"] = "\ue2af";
        dict["DetectorCo"] = "\ue2af";
        dict["detector_offline"] = "\ue223";
        dict["DetectorOffline"] = "\ue223";
        dict["detector_smoke"] = "\ue285";
        dict["DetectorSmoke"] = "\ue285";
        dict["detector_status"] = "\ue1e8";
        dict["DetectorStatus"] = "\ue1e8";
        dict["developer_board"] = "\ue30d";
        dict["DeveloperBoard"] = "\ue30d";
        dict["developer_board_off"] = "\ue4ff";
        dict["DeveloperBoardOff"] = "\ue4ff";
        dict["developer_guide"] = "\ue99e";
        dict["DeveloperGuide"] = "\ue99e";
        dict["developer_mode"] = "\uf2e2";
        dict["DeveloperMode"] = "\uf2e2";
        dict["developer_mode_tv"] = "\ue874";
        dict["DeveloperModeTv"] = "\ue874";
        dict["device_band"] = "\uf2f5";
        dict["DeviceBand"] = "\uf2f5";
        dict["device_hub"] = "\ue335";
        dict["DeviceHub"] = "\ue335";
        dict["device_reset"] = "\ue8b3";
        dict["DeviceReset"] = "\ue8b3";
        dict["device_swoosh_star"] = "\U000FFEB8";
        dict["DeviceSwooshStar"] = "\U000FFEB8";
        dict["device_thermostat"] = "\ue1ff";
        dict["DeviceThermostat"] = "\ue1ff";
        dict["device_unknown"] = "\uf2e1";
        dict["DeviceUnknown"] = "\uf2e1";
        dict["devices"] = "\ue326";
        dict["devices_fold"] = "\uebde";
        dict["DevicesFold"] = "\uebde";
        dict["devices_fold_2"] = "\uf406";
        dict["DevicesFold2"] = "\uf406";
        dict["devices_off"] = "\uf7a5";
        dict["DevicesOff"] = "\uf7a5";
        dict["devices_other"] = "\ue337";
        dict["DevicesOther"] = "\ue337";
        dict["devices_wearables"] = "\uf6ab";
        dict["DevicesWearables"] = "\uf6ab";
        dict["dew_point"] = "\uf879";
        dict["DewPoint"] = "\uf879";
        dict["diagnosis"] = "\ue0a8";
        dict["diagonal_line"] = "\uf41e";
        dict["DiagonalLine"] = "\uf41e";
        dict["dialer_sip"] = "\ue0bb";
        dict["DialerSip"] = "\ue0bb";
        dict["dialogs"] = "\ue99f";
        dict["dialpad"] = "\ue0bc";
        dict["diamond"] = "\uead5";
        dict["diamond_shine"] = "\uf2b2";
        dict["DiamondShine"] = "\uf2b2";
        dict["dictionary"] = "\uf539";
        dict["difference"] = "\ueb7d";
        dict["digital_out_of_home"] = "\uf1de";
        dict["DigitalOutOfHome"] = "\uf1de";
        dict["digital_wellbeing"] = "\uef86";
        dict["DigitalWellbeing"] = "\uef86";
        dict["dine_heart"] = "\uf29c";
        dict["DineHeart"] = "\uf29c";
        dict["dine_in"] = "\uf295";
        dict["DineIn"] = "\uf295";
        dict["dine_lamp"] = "\uf29b";
        dict["DineLamp"] = "\uf29b";
        dict["dining"] = "\ueff4";
        dict["dinner_dining"] = "\uea57";
        dict["DinnerDining"] = "\uea57";
        dict["directions"] = "\ue52e";
        dict["directions_alt"] = "\uf880";
        dict["DirectionsAlt"] = "\uf880";
        dict["directions_alt_off"] = "\uf881";
        dict["DirectionsAltOff"] = "\uf881";
        dict["directions_bike"] = "\ue52f";
        dict["DirectionsBike"] = "\ue52f";
        dict["directions_boat"] = "\ueff5";
        dict["DirectionsBoat"] = "\ueff5";
        dict["directions_boat_filled"] = "\ueff5";
        dict["DirectionsBoatFilled"] = "\ueff5";
        dict["directions_bus"] = "\ueff6";
        dict["DirectionsBus"] = "\ueff6";
        dict["directions_bus_filled"] = "\ueff6";
        dict["DirectionsBusFilled"] = "\ueff6";
        dict["directions_car"] = "\ueff7";
        dict["DirectionsCar"] = "\ueff7";
        dict["directions_car_filled"] = "\ueff7";
        dict["DirectionsCarFilled"] = "\ueff7";
        dict["directions_off"] = "\uf10f";
        dict["DirectionsOff"] = "\uf10f";
        dict["directions_railway"] = "\ueff8";
        dict["DirectionsRailway"] = "\ueff8";
        dict["directions_railway_2"] = "\uf462";
        dict["DirectionsRailway2"] = "\uf462";
        dict["directions_railway_filled"] = "\ueff8";
        dict["DirectionsRailwayFilled"] = "\ueff8";
        dict["directions_run"] = "\ue566";
        dict["DirectionsRun"] = "\ue566";
        dict["directions_subway"] = "\ueffa";
        dict["DirectionsSubway"] = "\ueffa";
        dict["directions_subway_filled"] = "\ueffa";
        dict["DirectionsSubwayFilled"] = "\ueffa";
        dict["directions_transit"] = "\ueffa";
        dict["DirectionsTransit"] = "\ueffa";
        dict["directions_transit_filled"] = "\ueffa";
        dict["DirectionsTransitFilled"] = "\ueffa";
        dict["directions_walk"] = "\ue536";
        dict["DirectionsWalk"] = "\ue536";
        dict["directory_sync"] = "\ue394";
        dict["DirectorySync"] = "\ue394";
        dict["dirty_lens"] = "\uef4b";
        dict["DirtyLens"] = "\uef4b";
        dict["disabled_by_default"] = "\uf230";
        dict["DisabledByDefault"] = "\uf230";
        dict["disabled_visible"] = "\ue76e";
        dict["DisabledVisible"] = "\ue76e";
        dict["disc_full"] = "\ue610";
        dict["DiscFull"] = "\ue610";
        dict["discover_tune"] = "\ue018";
        dict["DiscoverTune"] = "\ue018";
        dict["dishwasher"] = "\ue9a0";
        dict["dishwasher_gen"] = "\ue832";
        dict["DishwasherGen"] = "\ue832";
        dict["display_add"] = "\U000FFED2";
        dict["DisplayAdd"] = "\U000FFED2";
        dict["display_external_input"] = "\uf7e7";
        dict["DisplayExternalInput"] = "\uf7e7";
        dict["display_settings"] = "\ueb97";
        dict["DisplaySettings"] = "\ueb97";
        dict["distance"] = "\uf6ea";
        dict["diversity_1"] = "\uf8d7";
        dict["Diversity1"] = "\uf8d7";
        dict["diversity_2"] = "\uf8d8";
        dict["Diversity2"] = "\uf8d8";
        dict["diversity_3"] = "\uf8d9";
        dict["Diversity3"] = "\uf8d9";
        dict["diversity_4"] = "\uf857";
        dict["Diversity4"] = "\uf857";
        dict["dns"] = "\ue875";
        dict["do_disturb"] = "\uf08c";
        dict["DoDisturb"] = "\uf08c";
        dict["do_disturb_alt"] = "\uf08d";
        dict["DoDisturbAlt"] = "\uf08d";
        dict["do_disturb_off"] = "\uf08e";
        dict["DoDisturbOff"] = "\uf08e";
        dict["do_disturb_on"] = "\uf08f";
        dict["DoDisturbOn"] = "\uf08f";
        dict["do_not_disturb"] = "\uf08d";
        dict["DoNotDisturb"] = "\uf08d";
        dict["do_not_disturb_alt"] = "\uf08c";
        dict["DoNotDisturbAlt"] = "\uf08c";
        dict["do_not_disturb_off"] = "\uf08e";
        dict["DoNotDisturbOff"] = "\uf08e";
        dict["do_not_disturb_on"] = "\uf08f";
        dict["DoNotDisturbOn"] = "\uf08f";
        dict["do_not_disturb_on_total_silence"] = "\ueffb";
        dict["DoNotDisturbOnTotalSilence"] = "\ueffb";
        dict["do_not_step"] = "\uf19f";
        dict["DoNotStep"] = "\uf19f";
        dict["do_not_touch"] = "\uf1b0";
        dict["DoNotTouch"] = "\uf1b0";
        dict["dock"] = "\uf2e0";
        dict["dock_to_bottom"] = "\uf7e6";
        dict["DockToBottom"] = "\uf7e6";
        dict["dock_to_left"] = "\uf7e5";
        dict["DockToLeft"] = "\uf7e5";
        dict["dock_to_right"] = "\uf7e4";
        dict["DockToRight"] = "\uf7e4";
        dict["docs"] = "\uea7d";
        dict["docs_add_on"] = "\uf0c2";
        dict["DocsAddOn"] = "\uf0c2";
        dict["docs_apps_script"] = "\uf0c3";
        dict["DocsAppsScript"] = "\uf0c3";
        dict["document_scanner"] = "\ue5fa";
        dict["DocumentScanner"] = "\ue5fa";
        dict["document_search"] = "\uf385";
        dict["DocumentSearch"] = "\uf385";
        dict["domain"] = "\ue7ee";
        dict["domain_add"] = "\ueb62";
        dict["DomainAdd"] = "\ueb62";
        dict["domain_disabled"] = "\ue0ef";
        dict["DomainDisabled"] = "\ue0ef";
        dict["domain_disabled_check"] = "\U000FFEC6";
        dict["DomainDisabledCheck"] = "\U000FFEC6";
        dict["domain_verification"] = "\uef4c";
        dict["DomainVerification"] = "\uef4c";
        dict["domain_verification_off"] = "\uf7b0";
        dict["DomainVerificationOff"] = "\uf7b0";
        dict["domino_mask"] = "\uf5e4";
        dict["DominoMask"] = "\uf5e4";
        dict["done"] = "\ue876";
        dict["done_all"] = "\ue877";
        dict["DoneAll"] = "\ue877";
        dict["done_outline"] = "\ue92f";
        dict["DoneOutline"] = "\ue92f";
        dict["donut_large"] = "\ue917";
        dict["DonutLarge"] = "\ue917";
        dict["donut_small"] = "\ue918";
        dict["DonutSmall"] = "\ue918";
        dict["door_back"] = "\ueffc";
        dict["DoorBack"] = "\ueffc";
        dict["door_front"] = "\ueffd";
        dict["DoorFront"] = "\ueffd";
        dict["door_open"] = "\ue77c";
        dict["DoorOpen"] = "\ue77c";
        dict["door_sensor"] = "\ue28a";
        dict["DoorSensor"] = "\ue28a";
        dict["door_sliding"] = "\ueffe";
        dict["DoorSliding"] = "\ueffe";
        dict["doorbell"] = "\uefff";
        dict["doorbell_3p"] = "\ue1e7";
        dict["Doorbell3p"] = "\ue1e7";
        dict["doorbell_chime"] = "\ue1f3";
        dict["DoorbellChime"] = "\ue1f3";
        dict["double_arrow"] = "\uea50";
        dict["DoubleArrow"] = "\uea50";
        dict["downhill_skiing"] = "\ue509";
        dict["DownhillSkiing"] = "\ue509";
        dict["download"] = "\uf090";
        dict["download_2"] = "\uf523";
        dict["Download2"] = "\uf523";
        dict["download_done"] = "\uf091";
        dict["DownloadDone"] = "\uf091";
        dict["download_for_offline"] = "\uf000";
        dict["DownloadForOffline"] = "\uf000";
        dict["downloading"] = "\uf001";
        dict["draft"] = "\ue66d";
        dict["draft_orders"] = "\ue7b3";
        dict["DraftOrders"] = "\ue7b3";
        dict["drafts"] = "\ue151";
        dict["drag_click"] = "\uf71f";
        dict["DragClick"] = "\uf71f";
        dict["drag_handle"] = "\ue25d";
        dict["DragHandle"] = "\ue25d";
        dict["drag_indicator"] = "\ue945";
        dict["DragIndicator"] = "\ue945";
        dict["drag_pan"] = "\uf71e";
        dict["DragPan"] = "\uf71e";
        dict["draw"] = "\ue746";
        dict["draw_abstract"] = "\uf7f8";
        dict["DrawAbstract"] = "\uf7f8";
        dict["draw_collage"] = "\uf7f7";
        dict["DrawCollage"] = "\uf7f7";
        dict["drawing_recognition"] = "\ueb00";
        dict["DrawingRecognition"] = "\ueb00";
        dict["dresser"] = "\ue210";
        dict["drive_eta"] = "\ueff7";
        dict["DriveEta"] = "\ueff7";
        dict["drive_export"] = "\uf41d";
        dict["DriveExport"] = "\uf41d";
        dict["drive_file_move"] = "\ue9a1";
        dict["DriveFileMove"] = "\ue9a1";
        dict["drive_file_move_outline"] = "\ue9a1";
        dict["DriveFileMoveOutline"] = "\ue9a1";
        dict["drive_file_move_rtl"] = "\ue9a1";
        dict["DriveFileMoveRtl"] = "\ue9a1";
        dict["drive_file_rename"] = "\ue676";
        dict["DriveFileRename"] = "\ue676";
        dict["drive_file_rename_outline"] = "\ue9a2";
        dict["DriveFileRenameOutline"] = "\ue9a2";
        dict["drive_folder_upload"] = "\ue9a3";
        dict["DriveFolderUpload"] = "\ue9a3";
        dict["drone"] = "\uf25a";
        dict["drone_2"] = "\uf259";
        dict["Drone2"] = "\uf259";
        dict["dropdown"] = "\ue9a4";
        dict["dropdown_menu"] = "\U000FFEF0";
        dict["DropdownMenu"] = "\U000FFEF0";
        dict["dropper_eye"] = "\uf351";
        dict["DropperEye"] = "\uf351";
        dict["dry"] = "\uf1b3";
        dict["dry_cleaning"] = "\uea58";
        dict["DryCleaning"] = "\uea58";
        dict["dual_screen"] = "\uf6cf";
        dict["DualScreen"] = "\uf6cf";
        dict["duo"] = "\ue9a5";
        dict["dvr"] = "\ue1b2";
        dict["dynamic_feed"] = "\uea14";
        dict["DynamicFeed"] = "\uea14";
        dict["dynamic_form"] = "\uf1bf";
        dict["DynamicForm"] = "\uf1bf";
        dict["e911_avatar"] = "\uf11a";
        dict["E911Avatar"] = "\uf11a";
        dict["e911_emergency"] = "\uf119";
        dict["E911Emergency"] = "\uf119";
        dict["e_mobiledata"] = "\uf002";
        dict["EMobiledata"] = "\uf002";
        dict["e_mobiledata_badge"] = "\uf7e3";
        dict["EMobiledataBadge"] = "\uf7e3";
        dict["ear_sound"] = "\uf356";
        dict["EarSound"] = "\uf356";
        dict["earbud_case"] = "\uf327";
        dict["EarbudCase"] = "\uf327";
        dict["earbud_left"] = "\uf326";
        dict["EarbudLeft"] = "\uf326";
        dict["earbud_right"] = "\uf325";
        dict["EarbudRight"] = "\uf325";
        dict["earbuds"] = "\uf003";
        dict["earbuds_2"] = "\uf324";
        dict["Earbuds2"] = "\uf324";
        dict["earbuds_battery"] = "\uf004";
        dict["EarbudsBattery"] = "\uf004";
        dict["early_on"] = "\ue2ba";
        dict["EarlyOn"] = "\ue2ba";
        dict["earthquake"] = "\uf64f";
        dict["east"] = "\uf1df";
        dict["ecg"] = "\uf80f";
        dict["ecg_heart"] = "\uf6e9";
        dict["EcgHeart"] = "\uf6e9";
        dict["eco"] = "\uea35";
        dict["eda"] = "\uf6e8";
        dict["edgesensor_high"] = "\uf2ef";
        dict["EdgesensorHigh"] = "\uf2ef";
        dict["edgesensor_low"] = "\uf2ee";
        dict["EdgesensorLow"] = "\uf2ee";
        dict["edit"] = "\uf097";
        dict["edit_arrow_down"] = "\uf380";
        dict["EditArrowDown"] = "\uf380";
        dict["edit_arrow_up"] = "\uf37f";
        dict["EditArrowUp"] = "\uf37f";
        dict["edit_attributes"] = "\ue578";
        dict["EditAttributes"] = "\ue578";
        dict["edit_audio"] = "\uf42d";
        dict["EditAudio"] = "\uf42d";
        dict["edit_calendar"] = "\ue742";
        dict["EditCalendar"] = "\ue742";
        dict["edit_document"] = "\uf88c";
        dict["EditDocument"] = "\uf88c";
        dict["edit_location"] = "\ue568";
        dict["EditLocation"] = "\ue568";
        dict["edit_location_alt"] = "\ue1c5";
        dict["EditLocationAlt"] = "\ue1c5";
        dict["edit_note"] = "\ue745";
        dict["EditNote"] = "\ue745";
        dict["edit_notifications"] = "\ue525";
        dict["EditNotifications"] = "\ue525";
        dict["edit_off"] = "\ue950";
        dict["EditOff"] = "\ue950";
        dict["edit_road"] = "\uef4d";
        dict["EditRoad"] = "\uef4d";
        dict["edit_square"] = "\uf88d";
        dict["EditSquare"] = "\uf88d";
        dict["editor_choice"] = "\uf528";
        dict["EditorChoice"] = "\uf528";
        dict["egg"] = "\ueacc";
        dict["egg_alt"] = "\ueac8";
        dict["EggAlt"] = "\ueac8";
        dict["eject"] = "\ue8fb";
        dict["elderly"] = "\uf21a";
        dict["elderly_woman"] = "\ueb69";
        dict["ElderlyWoman"] = "\ueb69";
        dict["electric_bike"] = "\ueb1b";
        dict["ElectricBike"] = "\ueb1b";
        dict["electric_bolt"] = "\uec1c";
        dict["ElectricBolt"] = "\uec1c";
        dict["electric_car"] = "\ueb1c";
        dict["ElectricCar"] = "\ueb1c";
        dict["electric_meter"] = "\uec1b";
        dict["ElectricMeter"] = "\uec1b";
        dict["electric_moped"] = "\ueb1d";
        dict["ElectricMoped"] = "\ueb1d";
        dict["electric_rickshaw"] = "\ueb1e";
        dict["ElectricRickshaw"] = "\ueb1e";
        dict["electric_scooter"] = "\ueb1f";
        dict["ElectricScooter"] = "\ueb1f";
        dict["electrical_services"] = "\uf102";
        dict["ElectricalServices"] = "\uf102";
        dict["elevation"] = "\uf6e7";
        dict["elevator"] = "\uf1a0";
        dict["email"] = "\ue159";
        dict["emergency"] = "\ue1eb";
        dict["emergency_heat"] = "\uf15d";
        dict["EmergencyHeat"] = "\uf15d";
        dict["emergency_heat_2"] = "\uf4e5";
        dict["EmergencyHeat2"] = "\uf4e5";
        dict["emergency_home"] = "\ue82a";
        dict["EmergencyHome"] = "\ue82a";
        dict["emergency_recording"] = "\uebf4";
        dict["EmergencyRecording"] = "\uebf4";
        dict["emergency_share"] = "\uebf6";
        dict["EmergencyShare"] = "\uebf6";
        dict["emergency_share_off"] = "\uf59e";
        dict["EmergencyShareOff"] = "\uf59e";
        dict["emoji_emotions"] = "\uea22";
        dict["EmojiEmotions"] = "\uea22";
        dict["emoji_events"] = "\uea23";
        dict["EmojiEvents"] = "\uea23";
        dict["emoji_flags"] = "\uf0c6";
        dict["EmojiFlags"] = "\uf0c6";
        dict["emoji_food_beverage"] = "\uea1b";
        dict["EmojiFoodBeverage"] = "\uea1b";
        dict["emoji_language"] = "\uf4cd";
        dict["EmojiLanguage"] = "\uf4cd";
        dict["emoji_nature"] = "\uea1c";
        dict["EmojiNature"] = "\uea1c";
        dict["emoji_objects"] = "\uea24";
        dict["EmojiObjects"] = "\uea24";
        dict["emoji_people"] = "\uea1d";
        dict["EmojiPeople"] = "\uea1d";
        dict["emoji_symbols"] = "\uea1e";
        dict["EmojiSymbols"] = "\uea1e";
        dict["emoji_transportation"] = "\uea1f";
        dict["EmojiTransportation"] = "\uea1f";
        dict["emoticon"] = "\ue5f3";
        dict["empty_dashboard"] = "\uf844";
        dict["EmptyDashboard"] = "\uf844";
        dict["enable"] = "\uf188";
        dict["encrypted"] = "\ue593";
        dict["encrypted_add"] = "\uf429";
        dict["EncryptedAdd"] = "\uf429";
        dict["encrypted_add_circle"] = "\uf42a";
        dict["EncryptedAddCircle"] = "\uf42a";
        dict["encrypted_minus_circle"] = "\uf428";
        dict["EncryptedMinusCircle"] = "\uf428";
        dict["encrypted_off"] = "\uf427";
        dict["EncryptedOff"] = "\uf427";
        dict["endocrinology"] = "\ue0a9";
        dict["energy"] = "\ue9a6";
        dict["energy_program_saving"] = "\uf15f";
        dict["EnergyProgramSaving"] = "\uf15f";
        dict["energy_program_time_used"] = "\uf161";
        dict["EnergyProgramTimeUsed"] = "\uf161";
        dict["energy_savings_leaf"] = "\uec1a";
        dict["EnergySavingsLeaf"] = "\uec1a";
        dict["engineering"] = "\uea3d";
        dict["enhanced_encryption"] = "\ue63f";
        dict["EnhancedEncryption"] = "\ue63f";
        dict["ent"] = "\ue0aa";
        dict["enterprise"] = "\ue70e";
        dict["enterprise_off"] = "\ueb4d";
        dict["EnterpriseOff"] = "\ueb4d";
        dict["equal"] = "\uf77b";
        dict["equalizer"] = "\ue01d";
        dict["eraser_size_1"] = "\uf3fc";
        dict["EraserSize1"] = "\uf3fc";
        dict["eraser_size_2"] = "\uf3fb";
        dict["EraserSize2"] = "\uf3fb";
        dict["eraser_size_3"] = "\uf3fa";
        dict["EraserSize3"] = "\uf3fa";
        dict["eraser_size_4"] = "\uf3f9";
        dict["EraserSize4"] = "\uf3f9";
        dict["eraser_size_5"] = "\uf3f8";
        dict["EraserSize5"] = "\uf3f8";
        dict["error"] = "\uf8b6";
        dict["error_circle_rounded"] = "\uf8b6";
        dict["ErrorCircleRounded"] = "\uf8b6";
        dict["error_med"] = "\ue49b";
        dict["ErrorMed"] = "\ue49b";
        dict["error_outline"] = "\uf8b6";
        dict["ErrorOutline"] = "\uf8b6";
        dict["escalator"] = "\uf1a1";
        dict["escalator_warning"] = "\uf1ac";
        dict["EscalatorWarning"] = "\uf1ac";
        dict["euro"] = "\uea15";
        dict["euro_symbol"] = "\ue926";
        dict["EuroSymbol"] = "\ue926";
        dict["ev_charger"] = "\ue56d";
        dict["EvCharger"] = "\ue56d";
        dict["ev_mobiledata_badge"] = "\uf7e2";
        dict["EvMobiledataBadge"] = "\uf7e2";
        dict["ev_shadow"] = "\uef8f";
        dict["EvShadow"] = "\uef8f";
        dict["ev_shadow_add"] = "\uf580";
        dict["EvShadowAdd"] = "\uf580";
        dict["ev_shadow_minus"] = "\uf57f";
        dict["EvShadowMinus"] = "\uf57f";
        dict["ev_station"] = "\ue56d";
        dict["EvStation"] = "\ue56d";
        dict["event"] = "\ue878";
        dict["event_available"] = "\ue614";
        dict["EventAvailable"] = "\ue614";
        dict["event_busy"] = "\ue615";
        dict["EventBusy"] = "\ue615";
        dict["event_list"] = "\uf683";
        dict["EventList"] = "\uf683";
        dict["event_note"] = "\ue616";
        dict["EventNote"] = "\ue616";
        dict["event_repeat"] = "\ueb7b";
        dict["EventRepeat"] = "\ueb7b";
        dict["event_seat"] = "\ue903";
        dict["EventSeat"] = "\ue903";
        dict["event_upcoming"] = "\uf238";
        dict["EventUpcoming"] = "\uf238";
        dict["exclamation"] = "\uf22f";
        dict["exercise"] = "\uf6e6";
        dict["exit_to_app"] = "\ue879";
        dict["ExitToApp"] = "\ue879";
        dict["expand"] = "\ue94f";
        dict["expand_all"] = "\ue946";
        dict["ExpandAll"] = "\ue946";
        dict["expand_circle_down"] = "\ue7cd";
        dict["ExpandCircleDown"] = "\ue7cd";
        dict["expand_circle_right"] = "\uf591";
        dict["ExpandCircleRight"] = "\uf591";
        dict["expand_circle_up"] = "\uf5d2";
        dict["ExpandCircleUp"] = "\uf5d2";
        dict["expand_content"] = "\uf830";
        dict["ExpandContent"] = "\uf830";
        dict["expand_less"] = "\ue5ce";
        dict["ExpandLess"] = "\ue5ce";
        dict["expand_more"] = "\ue5cf";
        dict["ExpandMore"] = "\ue5cf";
        dict["expansion_panels"] = "\uef90";
        dict["ExpansionPanels"] = "\uef90";
        dict["expension_panels"] = "\uef90";
        dict["ExpensionPanels"] = "\uef90";
        dict["experiment"] = "\ue686";
        dict["explicit"] = "\ue01e";
        dict["explore"] = "\ue87a";
        dict["explore_nearby"] = "\ue538";
        dict["ExploreNearby"] = "\ue538";
        dict["explore_off"] = "\ue9a8";
        dict["ExploreOff"] = "\ue9a8";
        dict["explosion"] = "\uf685";
        dict["export_notes"] = "\ue0ac";
        dict["ExportNotes"] = "\ue0ac";
        dict["exposure"] = "\ue3f6";
        dict["exposure_neg_1"] = "\ue3cb";
        dict["ExposureNeg1"] = "\ue3cb";
        dict["exposure_neg_2"] = "\ue3cc";
        dict["ExposureNeg2"] = "\ue3cc";
        dict["exposure_plus_1"] = "\ue800";
        dict["ExposurePlus1"] = "\ue800";
        dict["exposure_plus_2"] = "\ue3ce";
        dict["ExposurePlus2"] = "\ue3ce";
        dict["exposure_zero"] = "\ue3cf";
        dict["ExposureZero"] = "\ue3cf";
        dict["extension"] = "\ue87b";
        dict["extension_off"] = "\ue4f5";
        dict["ExtensionOff"] = "\ue4f5";
        dict["eye_tracking"] = "\uf4c9";
        dict["EyeTracking"] = "\uf4c9";
        dict["eyebrow"] = "\ueeb3";
        dict["eyeglasses"] = "\uf6ee";
        dict["eyeglasses_2"] = "\uf2c7";
        dict["Eyeglasses2"] = "\uf2c7";
        dict["eyeglasses_2_sound"] = "\uf265";
        dict["Eyeglasses2Sound"] = "\uf265";
        dict["eyeglasses_3"] = "\U000FFEF1";
        dict["Eyeglasses3"] = "\U000FFEF1";
        dict["face"] = "\uf008";
        dict["face_2"] = "\uf8da";
        dict["Face2"] = "\uf8da";
        dict["face_3"] = "\uf8db";
        dict["Face3"] = "\uf8db";
        dict["face_4"] = "\uf8dc";
        dict["Face4"] = "\uf8dc";
        dict["face_5"] = "\uf8dd";
        dict["Face5"] = "\uf8dd";
        dict["face_6"] = "\uf8de";
        dict["Face6"] = "\uf8de";
        dict["face_down"] = "\uf402";
        dict["FaceDown"] = "\uf402";
        dict["face_left"] = "\uf401";
        dict["FaceLeft"] = "\uf401";
        dict["face_nod"] = "\uf400";
        dict["FaceNod"] = "\uf400";
        dict["face_retouching_natural"] = "\uef4e";
        dict["FaceRetouchingNatural"] = "\uef4e";
        dict["face_retouching_off"] = "\uf007";
        dict["FaceRetouchingOff"] = "\uf007";
        dict["face_right"] = "\uf3ff";
        dict["FaceRight"] = "\uf3ff";
        dict["face_shake"] = "\uf3fe";
        dict["FaceShake"] = "\uf3fe";
        dict["face_unlock"] = "\uf008";
        dict["FaceUnlock"] = "\uf008";
        dict["face_up"] = "\uf3fd";
        dict["FaceUp"] = "\uf3fd";
        dict["fact_check"] = "\uf0c5";
        dict["FactCheck"] = "\uf0c5";
        dict["factory"] = "\uebbc";
        dict["falling"] = "\uf60d";
        dict["familiar_face_and_zone"] = "\ue21c";
        dict["FamiliarFaceAndZone"] = "\ue21c";
        dict["family_group"] = "\ueef2";
        dict["FamilyGroup"] = "\ueef2";
        dict["family_history"] = "\ue0ad";
        dict["FamilyHistory"] = "\ue0ad";
        dict["family_home"] = "\ueb26";
        dict["FamilyHome"] = "\ueb26";
        dict["family_link"] = "\ueb19";
        dict["FamilyLink"] = "\ueb19";
        dict["family_restroom"] = "\uf1a2";
        dict["FamilyRestroom"] = "\uf1a2";
        dict["family_star"] = "\uf527";
        dict["FamilyStar"] = "\uf527";
        dict["fan_focus"] = "\uf334";
        dict["FanFocus"] = "\uf334";
        dict["fan_indirect"] = "\uf333";
        dict["FanIndirect"] = "\uf333";
        dict["farsight_digital"] = "\uf559";
        dict["FarsightDigital"] = "\uf559";
        dict["fast_forward"] = "\ue01f";
        dict["FastForward"] = "\ue01f";
        dict["fast_rewind"] = "\ue020";
        dict["FastRewind"] = "\ue020";
        dict["fastfood"] = "\ue57a";
        dict["faucet"] = "\ue278";
        dict["favorite"] = "\ue87e";
        dict["favorite_border"] = "\ue87e";
        dict["FavoriteBorder"] = "\ue87e";
        dict["fax"] = "\uead8";
        dict["feature_search"] = "\ue9a9";
        dict["FeatureSearch"] = "\ue9a9";
        dict["featured_play_list"] = "\ue06d";
        dict["FeaturedPlayList"] = "\ue06d";
        dict["featured_seasonal_and_gifts"] = "\uef91";
        dict["FeaturedSeasonalAndGifts"] = "\uef91";
        dict["featured_video"] = "\ue06e";
        dict["FeaturedVideo"] = "\ue06e";
        dict["feed"] = "\uf009";
        dict["feedback"] = "\ue87f";
        dict["female"] = "\ue590";
        dict["femur"] = "\uf891";
        dict["femur_alt"] = "\uf892";
        dict["FemurAlt"] = "\uf892";
        dict["fence"] = "\uf1f6";
        dict["fertile"] = "\uf6e5";
        dict["festival"] = "\uea68";
        dict["fiber_dvr"] = "\ue05d";
        dict["FiberDvr"] = "\ue05d";
        dict["fiber_manual_record"] = "\ue061";
        dict["FiberManualRecord"] = "\ue061";
        dict["fiber_new"] = "\ue05e";
        dict["FiberNew"] = "\ue05e";
        dict["fiber_pin"] = "\ue06a";
        dict["FiberPin"] = "\ue06a";
        dict["fiber_smart_record"] = "\ue062";
        dict["FiberSmartRecord"] = "\ue062";
        dict["file_copy"] = "\ue173";
        dict["FileCopy"] = "\ue173";
        dict["file_copy_off"] = "\uf4d8";
        dict["FileCopyOff"] = "\uf4d8";
        dict["file_download"] = "\uf090";
        dict["FileDownload"] = "\uf090";
        dict["file_download_done"] = "\uf091";
        dict["FileDownloadDone"] = "\uf091";
        dict["file_download_off"] = "\ue4fe";
        dict["FileDownloadOff"] = "\ue4fe";
        dict["file_export"] = "\uf3b2";
        dict["FileExport"] = "\uf3b2";
        dict["file_json"] = "\uf3bb";
        dict["FileJson"] = "\uf3bb";
        dict["file_map"] = "\ue2c5";
        dict["FileMap"] = "\ue2c5";
        dict["file_map_stack"] = "\uf3e2";
        dict["FileMapStack"] = "\uf3e2";
        dict["file_open"] = "\ueaf3";
        dict["FileOpen"] = "\ueaf3";
        dict["file_png"] = "\uf3bc";
        dict["FilePng"] = "\uf3bc";
        dict["file_present"] = "\uea0e";
        dict["FilePresent"] = "\uea0e";
        dict["file_save"] = "\uf17f";
        dict["FileSave"] = "\uf17f";
        dict["file_save_off"] = "\ue505";
        dict["FileSaveOff"] = "\ue505";
        dict["file_upload"] = "\uf09b";
        dict["FileUpload"] = "\uf09b";
        dict["file_upload_off"] = "\uf886";
        dict["FileUploadOff"] = "\uf886";
        dict["files"] = "\uea85";
        dict["filter"] = "\ue3d3";
        dict["filter_1"] = "\ue3d0";
        dict["Filter1"] = "\ue3d0";
        dict["filter_2"] = "\ue3d1";
        dict["Filter2"] = "\ue3d1";
        dict["filter_3"] = "\ue3d2";
        dict["Filter3"] = "\ue3d2";
        dict["filter_4"] = "\ue3d4";
        dict["Filter4"] = "\ue3d4";
        dict["filter_5"] = "\ue3d5";
        dict["Filter5"] = "\ue3d5";
        dict["filter_6"] = "\ue3d6";
        dict["Filter6"] = "\ue3d6";
        dict["filter_7"] = "\ue3d7";
        dict["Filter7"] = "\ue3d7";
        dict["filter_8"] = "\ue3d8";
        dict["Filter8"] = "\ue3d8";
        dict["filter_9"] = "\ue3d9";
        dict["Filter9"] = "\ue3d9";
        dict["filter_9_plus"] = "\ue3da";
        dict["Filter9Plus"] = "\ue3da";
        dict["filter_alt"] = "\uef4f";
        dict["FilterAlt"] = "\uef4f";
        dict["filter_alt_off"] = "\ueb32";
        dict["FilterAltOff"] = "\ueb32";
        dict["filter_arrow_right"] = "\uf3d1";
        dict["FilterArrowRight"] = "\uf3d1";
        dict["filter_b_and_w"] = "\ue3db";
        dict["FilterBAndW"] = "\ue3db";
        dict["filter_center_focus"] = "\ue3dc";
        dict["FilterCenterFocus"] = "\ue3dc";
        dict["filter_drama"] = "\ue3dd";
        dict["FilterDrama"] = "\ue3dd";
        dict["filter_frames"] = "\ue3de";
        dict["FilterFrames"] = "\ue3de";
        dict["filter_hdr"] = "\ue3df";
        dict["FilterHdr"] = "\ue3df";
        dict["filter_list"] = "\ue152";
        dict["FilterList"] = "\ue152";
        dict["filter_list_alt"] = "\ue94e";
        dict["FilterListAlt"] = "\ue94e";
        dict["filter_list_off"] = "\ueb57";
        dict["FilterListOff"] = "\ueb57";
        dict["filter_none"] = "\ue3e0";
        dict["FilterNone"] = "\ue3e0";
        dict["filter_retrolux"] = "\ue3e1";
        dict["FilterRetrolux"] = "\ue3e1";
        dict["filter_tilt_shift"] = "\ue3e2";
        dict["FilterTiltShift"] = "\ue3e2";
        dict["filter_vintage"] = "\ue3e3";
        dict["FilterVintage"] = "\ue3e3";
        dict["finance"] = "\ue6bf";
        dict["finance_chip"] = "\uf84e";
        dict["FinanceChip"] = "\uf84e";
        dict["finance_mode"] = "\uef92";
        dict["FinanceMode"] = "\uef92";
        dict["find_in_page"] = "\ue880";
        dict["FindInPage"] = "\ue880";
        dict["find_replace"] = "\ue881";
        dict["FindReplace"] = "\ue881";
        dict["fingerprint"] = "\ue90d";
        dict["fingerprint_off"] = "\uf49d";
        dict["FingerprintOff"] = "\uf49d";
        dict["fire_check"] = "\U000FFFA8";
        dict["FireCheck"] = "\U000FFFA8";
        dict["fire_extinguisher"] = "\uf1d8";
        dict["FireExtinguisher"] = "\uf1d8";
        dict["fire_hydrant"] = "\uf1a3";
        dict["FireHydrant"] = "\uf1a3";
        dict["fire_truck"] = "\uf8f2";
        dict["FireTruck"] = "\uf8f2";
        dict["fireplace"] = "\uea43";
        dict["first_page"] = "\ue5dc";
        dict["FirstPage"] = "\ue5dc";
        dict["fit_page"] = "\uf77a";
        dict["FitPage"] = "\uf77a";
        dict["fit_page_height"] = "\uf397";
        dict["FitPageHeight"] = "\uf397";
        dict["fit_page_width"] = "\uf396";
        dict["FitPageWidth"] = "\uf396";
        dict["fit_screen"] = "\uea10";
        dict["FitScreen"] = "\uea10";
        dict["fit_width"] = "\uf779";
        dict["FitWidth"] = "\uf779";
        dict["fitness_center"] = "\ueb43";
        dict["FitnessCenter"] = "\ueb43";
        dict["fitness_tracker"] = "\uf463";
        dict["FitnessTracker"] = "\uf463";
        dict["fitness_trackers"] = "\ueef1";
        dict["FitnessTrackers"] = "\ueef1";
        dict["flag"] = "\uf0c6";
        dict["flag_2"] = "\uf40f";
        dict["Flag2"] = "\uf40f";
        dict["flag_check"] = "\uf3d8";
        dict["FlagCheck"] = "\uf3d8";
        dict["flag_circle"] = "\ueaf8";
        dict["FlagCircle"] = "\ueaf8";
        dict["flag_filled"] = "\uf0c6";
        dict["FlagFilled"] = "\uf0c6";
        dict["flaky"] = "\uef50";
        dict["flare"] = "\ue3e4";
        dict["flash_auto"] = "\ue3e5";
        dict["FlashAuto"] = "\ue3e5";
        dict["flash_off"] = "\ue3e6";
        dict["FlashOff"] = "\ue3e6";
        dict["flash_on"] = "\ue3e7";
        dict["FlashOn"] = "\ue3e7";
        dict["flashlight_off"] = "\uf00a";
        dict["FlashlightOff"] = "\uf00a";
        dict["flashlight_on"] = "\uf00b";
        dict["FlashlightOn"] = "\uf00b";
        dict["flatware"] = "\uf00c";
        dict["flex_direction"] = "\uf778";
        dict["FlexDirection"] = "\uf778";
        dict["flex_no_wrap"] = "\uf777";
        dict["FlexNoWrap"] = "\uf777";
        dict["flex_wrap"] = "\uf776";
        dict["FlexWrap"] = "\uf776";
        dict["flight"] = "\ue539";
        dict["flight_class"] = "\ue7cb";
        dict["FlightClass"] = "\ue7cb";
        dict["flight_land"] = "\ue904";
        dict["FlightLand"] = "\ue904";
        dict["flight_takeoff"] = "\ue905";
        dict["FlightTakeoff"] = "\ue905";
        dict["flights_and_hotels"] = "\ue9ab";
        dict["FlightsAndHotels"] = "\ue9ab";
        dict["flightsmode"] = "\uef93";
        dict["flip"] = "\ue3e8";
        dict["flip_camera_android"] = "\uea37";
        dict["FlipCameraAndroid"] = "\uea37";
        dict["flip_camera_ios"] = "\uea38";
        dict["FlipCameraIos"] = "\uea38";
        dict["flip_to_back"] = "\ue882";
        dict["FlipToBack"] = "\ue882";
        dict["flip_to_front"] = "\ue883";
        dict["FlipToFront"] = "\ue883";
        dict["float_landscape_2"] = "\uf45c";
        dict["FloatLandscape2"] = "\uf45c";
        dict["float_portrait_2"] = "\uf45b";
        dict["FloatPortrait2"] = "\uf45b";
        dict["flood"] = "\uebe6";
        dict["floor"] = "\uf6e4";
        dict["floor_lamp"] = "\ue21e";
        dict["FloorLamp"] = "\ue21e";
        dict["flourescent"] = "\uf07d";
        dict["flowchart"] = "\uf38d";
        dict["flowsheet"] = "\ue0ae";
        dict["fluid"] = "\ue483";
        dict["fluid_balance"] = "\uf80d";
        dict["FluidBalance"] = "\uf80d";
        dict["fluid_med"] = "\uf80c";
        dict["FluidMed"] = "\uf80c";
        dict["fluorescent"] = "\uf07d";
        dict["flutter"] = "\uf1dd";
        dict["flutter_dash"] = "\ue00b";
        dict["FlutterDash"] = "\ue00b";
        dict["flyover"] = "\uf478";
        dict["fmd_bad"] = "\uf00e";
        dict["FmdBad"] = "\uf00e";
        dict["fmd_good"] = "\uf1db";
        dict["FmdGood"] = "\uf1db";
        dict["foggy"] = "\ue818";
        dict["folded_hands"] = "\uf5ed";
        dict["FoldedHands"] = "\uf5ed";
        dict["folder"] = "\ue2c7";
        dict["folder_check"] = "\uf3d7";
        dict["FolderCheck"] = "\uf3d7";
        dict["folder_check_2"] = "\uf3d6";
        dict["FolderCheck2"] = "\uf3d6";
        dict["folder_code"] = "\uf3c8";
        dict["FolderCode"] = "\uf3c8";
        dict["folder_copy"] = "\uebbd";
        dict["FolderCopy"] = "\uebbd";
        dict["folder_data"] = "\uf586";
        dict["FolderData"] = "\uf586";
        dict["folder_delete"] = "\ueb34";
        dict["FolderDelete"] = "\ueb34";
        dict["folder_eye"] = "\uf3d5";
        dict["FolderEye"] = "\uf3d5";
        dict["folder_info"] = "\uf395";
        dict["FolderInfo"] = "\uf395";
        dict["folder_limited"] = "\uf4e4";
        dict["FolderLimited"] = "\uf4e4";
        dict["folder_managed"] = "\uf775";
        dict["FolderManaged"] = "\uf775";
        dict["folder_match"] = "\uf3d4";
        dict["FolderMatch"] = "\uf3d4";
        dict["folder_off"] = "\ueb83";
        dict["FolderOff"] = "\ueb83";
        dict["folder_open"] = "\ue2c8";
        dict["FolderOpen"] = "\ue2c8";
        dict["folder_shared"] = "\ue2c9";
        dict["FolderShared"] = "\ue2c9";
        dict["folder_special"] = "\ue617";
        dict["FolderSpecial"] = "\ue617";
        dict["folder_supervised"] = "\uf774";
        dict["FolderSupervised"] = "\uf774";
        dict["folder_zip"] = "\ueb2c";
        dict["FolderZip"] = "\ueb2c";
        dict["follow_the_signs"] = "\uf222";
        dict["FollowTheSigns"] = "\uf222";
        dict["font_download"] = "\ue167";
        dict["FontDownload"] = "\ue167";
        dict["font_download_off"] = "\ue4f9";
        dict["FontDownloadOff"] = "\ue4f9";
        dict["food_bank"] = "\uf1f2";
        dict["FoodBank"] = "\uf1f2";
        dict["foot_bones"] = "\uf893";
        dict["FootBones"] = "\uf893";
        dict["footprint"] = "\uf87d";
        dict["for_you"] = "\ue9ac";
        dict["ForYou"] = "\ue9ac";
        dict["forest"] = "\uea99";
        dict["fork_chart"] = "\U000FFFA6";
        dict["ForkChart"] = "\U000FFFA6";
        dict["fork_left"] = "\ueba0";
        dict["ForkLeft"] = "\ueba0";
        dict["fork_right"] = "\uebac";
        dict["ForkRight"] = "\uebac";
        dict["fork_spoon"] = "\uf3e4";
        dict["ForkSpoon"] = "\uf3e4";
        dict["forklift"] = "\uf868";
        dict["format_align_center"] = "\ue234";
        dict["FormatAlignCenter"] = "\ue234";
        dict["format_align_justify"] = "\ue235";
        dict["FormatAlignJustify"] = "\ue235";
        dict["format_align_left"] = "\ue236";
        dict["FormatAlignLeft"] = "\ue236";
        dict["format_align_right"] = "\ue237";
        dict["FormatAlignRight"] = "\ue237";
        dict["format_bold"] = "\ue238";
        dict["FormatBold"] = "\ue238";
        dict["format_clear"] = "\ue239";
        dict["FormatClear"] = "\ue239";
        dict["format_color_fill"] = "\ue23a";
        dict["FormatColorFill"] = "\ue23a";
        dict["format_color_reset"] = "\ue23b";
        dict["FormatColorReset"] = "\ue23b";
        dict["format_color_text"] = "\ue23c";
        dict["FormatColorText"] = "\ue23c";
        dict["format_h1"] = "\uf85d";
        dict["FormatH1"] = "\uf85d";
        dict["format_h2"] = "\uf85e";
        dict["FormatH2"] = "\uf85e";
        dict["format_h3"] = "\uf85f";
        dict["FormatH3"] = "\uf85f";
        dict["format_h4"] = "\uf860";
        dict["FormatH4"] = "\uf860";
        dict["format_h5"] = "\uf861";
        dict["FormatH5"] = "\uf861";
        dict["format_h6"] = "\uf862";
        dict["FormatH6"] = "\uf862";
        dict["format_image_back"] = "\ueeb0";
        dict["FormatImageBack"] = "\ueeb0";
        dict["format_image_break_left"] = "\ueeaf";
        dict["FormatImageBreakLeft"] = "\ueeaf";
        dict["format_image_break_right"] = "\ueeae";
        dict["FormatImageBreakRight"] = "\ueeae";
        dict["format_image_front"] = "\ueead";
        dict["FormatImageFront"] = "\ueead";
        dict["format_image_inline_left"] = "\ueeac";
        dict["FormatImageInlineLeft"] = "\ueeac";
        dict["format_image_inline_right"] = "\U000FFFFD";
        dict["FormatImageInlineRight"] = "\U000FFFFD";
        dict["format_image_left"] = "\uf863";
        dict["FormatImageLeft"] = "\uf863";
        dict["format_image_right"] = "\uf864";
        dict["FormatImageRight"] = "\uf864";
        dict["format_indent_decrease"] = "\ue23d";
        dict["FormatIndentDecrease"] = "\ue23d";
        dict["format_indent_increase"] = "\ue23e";
        dict["FormatIndentIncrease"] = "\ue23e";
        dict["format_ink_highlighter"] = "\uf82b";
        dict["FormatInkHighlighter"] = "\uf82b";
        dict["format_italic"] = "\ue23f";
        dict["FormatItalic"] = "\ue23f";
        dict["format_letter_spacing"] = "\uf773";
        dict["FormatLetterSpacing"] = "\uf773";
        dict["format_letter_spacing_2"] = "\uf618";
        dict["FormatLetterSpacing2"] = "\uf618";
        dict["format_letter_spacing_standard"] = "\uf617";
        dict["FormatLetterSpacingStandard"] = "\uf617";
        dict["format_letter_spacing_wide"] = "\uf616";
        dict["FormatLetterSpacingWide"] = "\uf616";
        dict["format_letter_spacing_wider"] = "\uf615";
        dict["FormatLetterSpacingWider"] = "\uf615";
        dict["format_line_spacing"] = "\ue240";
        dict["FormatLineSpacing"] = "\ue240";
        dict["format_list_bulleted"] = "\ue241";
        dict["FormatListBulleted"] = "\ue241";
        dict["format_list_bulleted_add"] = "\uf849";
        dict["FormatListBulletedAdd"] = "\uf849";
        dict["format_list_numbered"] = "\ue242";
        dict["FormatListNumbered"] = "\ue242";
        dict["format_list_numbered_rtl"] = "\ue267";
        dict["FormatListNumberedRtl"] = "\ue267";
        dict["format_overline"] = "\ueb65";
        dict["FormatOverline"] = "\ueb65";
        dict["format_paint"] = "\ue243";
        dict["FormatPaint"] = "\ue243";
        dict["format_paint_off"] = "\U000FFF97";
        dict["FormatPaintOff"] = "\U000FFF97";
        dict["format_paragraph"] = "\uf865";
        dict["FormatParagraph"] = "\uf865";
        dict["format_quote"] = "\ue244";
        dict["FormatQuote"] = "\ue244";
        dict["format_quote_off"] = "\uf413";
        dict["FormatQuoteOff"] = "\uf413";
        dict["format_shapes"] = "\ue25e";
        dict["FormatShapes"] = "\ue25e";
        dict["format_size"] = "\ue245";
        dict["FormatSize"] = "\ue245";
        dict["format_strikethrough"] = "\ue246";
        dict["FormatStrikethrough"] = "\ue246";
        dict["format_text_clip"] = "\uf82a";
        dict["FormatTextClip"] = "\uf82a";
        dict["format_text_overflow"] = "\uf829";
        dict["FormatTextOverflow"] = "\uf829";
        dict["format_text_wrap"] = "\uf828";
        dict["FormatTextWrap"] = "\uf828";
        dict["format_textdirection_l_to_r"] = "\ue247";
        dict["FormatTextdirectionLToR"] = "\ue247";
        dict["format_textdirection_r_to_l"] = "\ue248";
        dict["FormatTextdirectionRToL"] = "\ue248";
        dict["format_textdirection_vertical"] = "\uf4b8";
        dict["FormatTextdirectionVertical"] = "\uf4b8";
        dict["format_underlined"] = "\ue249";
        dict["FormatUnderlined"] = "\ue249";
        dict["format_underlined_squiggle"] = "\uf885";
        dict["FormatUnderlinedSquiggle"] = "\uf885";
        dict["forms_add_on"] = "\uf0c7";
        dict["FormsAddOn"] = "\uf0c7";
        dict["forms_apps_script"] = "\uf0c8";
        dict["FormsAppsScript"] = "\uf0c8";
        dict["fort"] = "\ueaad";
        dict["forum"] = "\ue8af";
        dict["forward"] = "\uf57a";
        dict["forward_10"] = "\ue056";
        dict["Forward10"] = "\ue056";
        dict["forward_30"] = "\ue057";
        dict["Forward30"] = "\ue057";
        dict["forward_5"] = "\ue058";
        dict["Forward5"] = "\ue058";
        dict["forward_circle"] = "\uf6f5";
        dict["ForwardCircle"] = "\uf6f5";
        dict["forward_media"] = "\uf6f4";
        dict["ForwardMedia"] = "\uf6f4";
        dict["forward_to_inbox"] = "\uf187";
        dict["ForwardToInbox"] = "\uf187";
        dict["foundation"] = "\uf200";
        dict["fragrance"] = "\uf345";
        dict["frame_bug"] = "\ueeef";
        dict["FrameBug"] = "\ueeef";
        dict["frame_exclamation"] = "\ueeee";
        dict["FrameExclamation"] = "\ueeee";
        dict["frame_inspect"] = "\uf772";
        dict["FrameInspect"] = "\uf772";
        dict["frame_person"] = "\uf8a6";
        dict["FramePerson"] = "\uf8a6";
        dict["frame_person_mic"] = "\uf4d5";
        dict["FramePersonMic"] = "\uf4d5";
        dict["frame_person_off"] = "\uf7d1";
        dict["FramePersonOff"] = "\uf7d1";
        dict["frame_reload"] = "\uf771";
        dict["FrameReload"] = "\uf771";
        dict["frame_source"] = "\uf770";
        dict["FrameSource"] = "\uf770";
        dict["free_breakfast"] = "\ueb44";
        dict["FreeBreakfast"] = "\ueb44";
        dict["free_cancellation"] = "\ue748";
        dict["FreeCancellation"] = "\ue748";
        dict["front_hand"] = "\ue769";
        dict["FrontHand"] = "\ue769";
        dict["front_loader"] = "\uf869";
        dict["FrontLoader"] = "\uf869";
        dict["full_coverage"] = "\ueb12";
        dict["FullCoverage"] = "\ueb12";
        dict["full_hd"] = "\uf58b";
        dict["FullHd"] = "\uf58b";
        dict["full_stacked_bar_chart"] = "\uf212";
        dict["FullStackedBarChart"] = "\uf212";
        dict["fullscreen"] = "\ue5d0";
        dict["fullscreen_exit"] = "\ue5d1";
        dict["FullscreenExit"] = "\ue5d1";
        dict["fullscreen_portrait"] = "\uf45a";
        dict["FullscreenPortrait"] = "\uf45a";
        dict["function"] = "\uf866";
        dict["functions"] = "\ue24a";
        dict["funicular"] = "\uf477";
        dict["g_mobiledata"] = "\uf010";
        dict["GMobiledata"] = "\uf010";
        dict["g_mobiledata_badge"] = "\uf7e1";
        dict["GMobiledataBadge"] = "\uf7e1";
        dict["g_translate"] = "\ue927";
        dict["GTranslate"] = "\ue927";
        dict["gallery_thumbnail"] = "\uf86f";
        dict["GalleryThumbnail"] = "\uf86f";
        dict["game_bumper_left"] = "\ueee0";
        dict["GameBumperLeft"] = "\ueee0";
        dict["game_bumper_right"] = "\ueedf";
        dict["GameBumperRight"] = "\ueedf";
        dict["game_button_l"] = "\ueede";
        dict["GameButtonL"] = "\ueede";
        dict["game_button_l1"] = "\ueedd";
        dict["GameButtonL1"] = "\ueedd";
        dict["game_button_l2"] = "\ueedc";
        dict["GameButtonL2"] = "\ueedc";
        dict["game_button_r"] = "\ueedb";
        dict["GameButtonR"] = "\ueedb";
        dict["game_button_r1"] = "\ueeda";
        dict["GameButtonR1"] = "\ueeda";
        dict["game_button_r2"] = "\ueed9";
        dict["GameButtonR2"] = "\ueed9";
        dict["game_button_zl"] = "\ueed8";
        dict["GameButtonZl"] = "\ueed8";
        dict["game_button_zr"] = "\ueed7";
        dict["GameButtonZr"] = "\ueed7";
        dict["game_stick_l3"] = "\ueed6";
        dict["GameStickL3"] = "\ueed6";
        dict["game_stick_left"] = "\ueed5";
        dict["GameStickLeft"] = "\ueed5";
        dict["game_stick_r3"] = "\ueed4";
        dict["GameStickR3"] = "\ueed4";
        dict["game_stick_right"] = "\ueed3";
        dict["GameStickRight"] = "\ueed3";
        dict["game_trigger_left"] = "\ueed2";
        dict["GameTriggerLeft"] = "\ueed2";
        dict["game_trigger_right"] = "\ueed1";
        dict["GameTriggerRight"] = "\ueed1";
        dict["gamepad"] = "\ue30f";
        dict["gamepad_circle_down"] = "\ueed0";
        dict["GamepadCircleDown"] = "\ueed0";
        dict["gamepad_circle_left"] = "\ueecf";
        dict["GamepadCircleLeft"] = "\ueecf";
        dict["gamepad_circle_right"] = "\ueece";
        dict["GamepadCircleRight"] = "\ueece";
        dict["gamepad_circle_up"] = "\ueecd";
        dict["GamepadCircleUp"] = "\ueecd";
        dict["gamepad_down"] = "\ueecc";
        dict["GamepadDown"] = "\ueecc";
        dict["gamepad_left"] = "\ueecb";
        dict["GamepadLeft"] = "\ueecb";
        dict["gamepad_right"] = "\ueeca";
        dict["GamepadRight"] = "\ueeca";
        dict["gamepad_up"] = "\ueec9";
        dict["GamepadUp"] = "\ueec9";
        dict["games"] = "\ue30f";
        dict["garage"] = "\uf011";
        dict["garage_check"] = "\uf28d";
        dict["GarageCheck"] = "\uf28d";
        dict["garage_door"] = "\ue714";
        dict["GarageDoor"] = "\ue714";
        dict["garage_door_open"] = "\U000FFF77";
        dict["GarageDoorOpen"] = "\U000FFF77";
        dict["garage_home"] = "\ue82d";
        dict["GarageHome"] = "\ue82d";
        dict["garage_money"] = "\uf28c";
        dict["GarageMoney"] = "\uf28c";
        dict["garden_cart"] = "\uf8a9";
        dict["GardenCart"] = "\uf8a9";
        dict["gas_meter"] = "\uec19";
        dict["GasMeter"] = "\uec19";
        dict["gastroenterology"] = "\ue0f1";
        dict["gate"] = "\ue277";
        dict["gavel"] = "\ue90e";
        dict["general_device"] = "\ue6de";
        dict["GeneralDevice"] = "\ue6de";
        dict["generating_tokens"] = "\ue749";
        dict["GeneratingTokens"] = "\ue749";
        dict["genetics"] = "\ue0f3";
        dict["genres"] = "\ue6ee";
        dict["gesture"] = "\ue155";
        dict["gesture_select"] = "\uf657";
        dict["GestureSelect"] = "\uf657";
        dict["get_app"] = "\uf090";
        dict["GetApp"] = "\uf090";
        dict["gif"] = "\ue908";
        dict["gif_2"] = "\uf40e";
        dict["Gif2"] = "\uf40e";
        dict["gif_box"] = "\ue7a3";
        dict["GifBox"] = "\ue7a3";
        dict["girl"] = "\ueb68";
        dict["gite"] = "\ue58b";
        dict["glass_cup"] = "\uf6e3";
        dict["GlassCup"] = "\uf6e3";
        dict["globe"] = "\ue64c";
        dict["globe_2_cancel"] = "\U000FFFB7";
        dict["Globe2Cancel"] = "\U000FFFB7";
        dict["globe_2_question"] = "\U000FFFB6";
        dict["Globe2Question"] = "\U000FFFB6";
        dict["globe_asia"] = "\uf799";
        dict["GlobeAsia"] = "\uf799";
        dict["globe_book"] = "\uf3c9";
        dict["GlobeBook"] = "\uf3c9";
        dict["globe_clock"] = "\U000FFED1";
        dict["GlobeClock"] = "\U000FFED1";
        dict["globe_location_pin"] = "\uf35d";
        dict["GlobeLocationPin"] = "\uf35d";
        dict["globe_uk"] = "\uf798";
        dict["GlobeUk"] = "\uf798";
        dict["glucose"] = "\ue4a0";
        dict["glyphs"] = "\uf8a3";
        dict["go_to_line"] = "\uf71d";
        dict["GoToLine"] = "\uf71d";
        dict["golf_course"] = "\ueb45";
        dict["GolfCourse"] = "\ueb45";
        dict["gondola_lift"] = "\uf476";
        dict["GondolaLift"] = "\uf476";
        dict["google_home_devices"] = "\ue715";
        dict["GoogleHomeDevices"] = "\ue715";
        dict["google_plus_reshare"] = "\uf57a";
        dict["GooglePlusReshare"] = "\uf57a";
        dict["google_tv_remote"] = "\uf5db";
        dict["GoogleTvRemote"] = "\uf5db";
        dict["google_wifi"] = "\uf579";
        dict["GoogleWifi"] = "\uf579";
        dict["gpp_bad"] = "\uf012";
        dict["GppBad"] = "\uf012";
        dict["gpp_good"] = "\uf013";
        dict["GppGood"] = "\uf013";
        dict["gpp_maybe"] = "\uf014";
        dict["GppMaybe"] = "\uf014";
        dict["gps_fixed"] = "\ue55c";
        dict["GpsFixed"] = "\ue55c";
        dict["gps_not_fixed"] = "\ue1b7";
        dict["GpsNotFixed"] = "\ue1b7";
        dict["gps_off"] = "\ue1b6";
        dict["GpsOff"] = "\ue1b6";
        dict["grade"] = "\uf09a";
        dict["gradient"] = "\ue3e9";
        dict["grading"] = "\uea4f";
        dict["grain"] = "\ue3ea";
        dict["graph_1"] = "\uf3a0";
        dict["Graph1"] = "\uf3a0";
        dict["graph_2"] = "\uf39f";
        dict["Graph2"] = "\uf39f";
        dict["graph_3"] = "\uf39e";
        dict["Graph3"] = "\uf39e";
        dict["graph_4"] = "\uf39d";
        dict["Graph4"] = "\uf39d";
        dict["graph_5"] = "\uf39c";
        dict["Graph5"] = "\uf39c";
        dict["graph_6"] = "\uf39b";
        dict["Graph6"] = "\uf39b";
        dict["graph_7"] = "\uf346";
        dict["Graph7"] = "\uf346";
        dict["graph_8"] = "\U000FFFEC";
        dict["Graph8"] = "\U000FFFEC";
        dict["graphic_eq"] = "\ue1b8";
        dict["GraphicEq"] = "\ue1b8";
        dict["graphic_eq_off"] = "\U000FFF98";
        dict["GraphicEqOff"] = "\U000FFF98";
        dict["grass"] = "\uf205";
        dict["grid_3x3"] = "\uf015";
        dict["Grid3x3"] = "\uf015";
        dict["grid_3x3_off"] = "\uf67c";
        dict["Grid3x3Off"] = "\uf67c";
        dict["grid_4x4"] = "\uf016";
        dict["Grid4x4"] = "\uf016";
        dict["grid_goldenratio"] = "\uf017";
        dict["GridGoldenratio"] = "\uf017";
        dict["grid_guides"] = "\uf76f";
        dict["GridGuides"] = "\uf76f";
        dict["grid_layout_side"] = "\U000FFF8D";
        dict["GridLayoutSide"] = "\U000FFF8D";
        dict["grid_off"] = "\ue3eb";
        dict["GridOff"] = "\ue3eb";
        dict["grid_on"] = "\ue3ec";
        dict["GridOn"] = "\ue3ec";
        dict["grid_view"] = "\ue9b0";
        dict["GridView"] = "\ue9b0";
        dict["grocery"] = "\uef97";
        dict["group"] = "\uea21";
        dict["group_add"] = "\ue7f0";
        dict["GroupAdd"] = "\ue7f0";
        dict["group_off"] = "\ue747";
        dict["GroupOff"] = "\ue747";
        dict["group_remove"] = "\ue7ad";
        dict["GroupRemove"] = "\ue7ad";
        dict["group_search"] = "\uf3ce";
        dict["GroupSearch"] = "\uf3ce";
        dict["group_work"] = "\ue886";
        dict["GroupWork"] = "\ue886";
        dict["grouped_bar_chart"] = "\uf211";
        dict["GroupedBarChart"] = "\uf211";
        dict["groups"] = "\uf233";
        dict["groups_2"] = "\uf8df";
        dict["Groups2"] = "\uf8df";
        dict["groups_3"] = "\uf8e0";
        dict["Groups3"] = "\uf8e0";
        dict["guardian"] = "\uf4c1";
        dict["gynecology"] = "\ue0f4";
        dict["h_mobiledata"] = "\uf018";
        dict["HMobiledata"] = "\uf018";
        dict["h_mobiledata_badge"] = "\uf7e0";
        dict["HMobiledataBadge"] = "\uf7e0";
        dict["h_plus_mobiledata"] = "\uf019";
        dict["HPlusMobiledata"] = "\uf019";
        dict["h_plus_mobiledata_badge"] = "\uf7df";
        dict["HPlusMobiledataBadge"] = "\uf7df";
        dict["hail"] = "\ue9b1";
        dict["hallway"] = "\ue6f8";
        dict["hanami_dango"] = "\uf23f";
        dict["HanamiDango"] = "\uf23f";
        dict["hand_bones"] = "\uf894";
        dict["HandBones"] = "\uf894";
        dict["hand_gesture"] = "\uef9c";
        dict["HandGesture"] = "\uef9c";
        dict["hand_gesture_off"] = "\uf3f3";
        dict["HandGestureOff"] = "\uf3f3";
        dict["hand_meal"] = "\uf294";
        dict["HandMeal"] = "\uf294";
        dict["hand_package"] = "\uf293";
        dict["HandPackage"] = "\uf293";
        dict["handheld_controller"] = "\uf4c6";
        dict["HandheldController"] = "\uf4c6";
        dict["handshake"] = "\uebcb";
        dict["handwriting_recognition"] = "\ueb02";
        dict["HandwritingRecognition"] = "\ueb02";
        dict["handyman"] = "\uf10b";
        dict["hangout_video"] = "\ue0c1";
        dict["HangoutVideo"] = "\ue0c1";
        dict["hangout_video_off"] = "\ue0c2";
        dict["HangoutVideoOff"] = "\ue0c2";
        dict["hard_disk"] = "\uf3da";
        dict["HardDisk"] = "\uf3da";
        dict["hard_drive"] = "\uf80e";
        dict["HardDrive"] = "\uf80e";
        dict["hard_drive_2"] = "\uf7a4";
        dict["HardDrive2"] = "\uf7a4";
        dict["hardware"] = "\uea59";
        dict["hd"] = "\ue052";
        dict["hdr_auto"] = "\uf01a";
        dict["HdrAuto"] = "\uf01a";
        dict["hdr_auto_select"] = "\uf01b";
        dict["HdrAutoSelect"] = "\uf01b";
        dict["hdr_enhanced_select"] = "\uef51";
        dict["HdrEnhancedSelect"] = "\uef51";
        dict["hdr_off"] = "\ue3ed";
        dict["HdrOff"] = "\ue3ed";
        dict["hdr_off_select"] = "\uf01c";
        dict["HdrOffSelect"] = "\uf01c";
        dict["hdr_on"] = "\ue3ee";
        dict["HdrOn"] = "\ue3ee";
        dict["hdr_on_select"] = "\uf01d";
        dict["HdrOnSelect"] = "\uf01d";
        dict["hdr_plus"] = "\uf01e";
        dict["HdrPlus"] = "\uf01e";
        dict["hdr_plus_off"] = "\ue3ef";
        dict["HdrPlusOff"] = "\ue3ef";
        dict["hdr_strong"] = "\ue3f1";
        dict["HdrStrong"] = "\ue3f1";
        dict["hdr_weak"] = "\ue3f2";
        dict["HdrWeak"] = "\ue3f2";
        dict["head_mounted_device"] = "\uf4c5";
        dict["HeadMountedDevice"] = "\uf4c5";
        dict["headphones"] = "\uf01f";
        dict["headphones_battery"] = "\uf020";
        dict["HeadphonesBattery"] = "\uf020";
        dict["headset"] = "\uf01f";
        dict["headset_mic"] = "\ue311";
        dict["HeadsetMic"] = "\ue311";
        dict["headset_off"] = "\ue33a";
        dict["HeadsetOff"] = "\ue33a";
        dict["healing"] = "\ue3f3";
        dict["health_and_beauty"] = "\uef9d";
        dict["HealthAndBeauty"] = "\uef9d";
        dict["health_and_safety"] = "\ue1d5";
        dict["HealthAndSafety"] = "\ue1d5";
        dict["health_cross"] = "\uf2c3";
        dict["HealthCross"] = "\uf2c3";
        dict["health_metrics"] = "\uf6e2";
        dict["HealthMetrics"] = "\uf6e2";
        dict["heap_snapshot_large"] = "\uf76e";
        dict["HeapSnapshotLarge"] = "\uf76e";
        dict["heap_snapshot_multiple"] = "\uf76d";
        dict["HeapSnapshotMultiple"] = "\uf76d";
        dict["heap_snapshot_thumbnail"] = "\uf76c";
        dict["HeapSnapshotThumbnail"] = "\uf76c";
        dict["hearing"] = "\ue023";
        dict["hearing_aid"] = "\uf464";
        dict["HearingAid"] = "\uf464";
        dict["hearing_aid_disabled"] = "\uf3b0";
        dict["HearingAidDisabled"] = "\uf3b0";
        dict["hearing_aid_disabled_left"] = "\uf2ec";
        dict["HearingAidDisabledLeft"] = "\uf2ec";
        dict["hearing_aid_left"] = "\uf2ed";
        dict["HearingAidLeft"] = "\uf2ed";
        dict["hearing_disabled"] = "\uf104";
        dict["HearingDisabled"] = "\uf104";
        dict["heart_broken"] = "\ueac2";
        dict["HeartBroken"] = "\ueac2";
        dict["heart_check"] = "\uf60a";
        dict["HeartCheck"] = "\uf60a";
        dict["heart_minus"] = "\uf883";
        dict["HeartMinus"] = "\uf883";
        dict["heart_plus"] = "\uf884";
        dict["HeartPlus"] = "\uf884";
        dict["heart_smile"] = "\uf292";
        dict["HeartSmile"] = "\uf292";
        dict["heat"] = "\uf537";
        dict["heat_pump"] = "\uec18";
        dict["HeatPump"] = "\uec18";
        dict["heat_pump_balance"] = "\ue27e";
        dict["HeatPumpBalance"] = "\ue27e";
        dict["height"] = "\uea16";
        dict["helicopter"] = "\uf60c";
        dict["help"] = "\ue8fd";
        dict["help_center"] = "\uf1c0";
        dict["HelpCenter"] = "\uf1c0";
        dict["help_clinic"] = "\uf810";
        dict["HelpClinic"] = "\uf810";
        dict["help_outline"] = "\ue8fd";
        dict["HelpOutline"] = "\ue8fd";
        dict["hematology"] = "\ue0f6";
        dict["hevc"] = "\uf021";
        dict["hexagon"] = "\ueb39";
        dict["hide"] = "\uef9e";
        dict["hide_image"] = "\uf022";
        dict["HideImage"] = "\uf022";
        dict["hide_source"] = "\uf023";
        dict["HideSource"] = "\uf023";
        dict["high_chair"] = "\uf29a";
        dict["HighChair"] = "\uf29a";
        dict["high_density"] = "\uf79c";
        dict["HighDensity"] = "\uf79c";
        dict["high_quality"] = "\ue024";
        dict["HighQuality"] = "\ue024";
        dict["high_quality_off"] = "\U000FFED6";
        dict["HighQualityOff"] = "\U000FFED6";
        dict["high_res"] = "\uf54b";
        dict["HighRes"] = "\uf54b";
        dict["highlight"] = "\ue25f";
        dict["highlight_alt"] = "\uef52";
        dict["HighlightAlt"] = "\uef52";
        dict["highlight_keyboard_focus"] = "\uf510";
        dict["HighlightKeyboardFocus"] = "\uf510";
        dict["highlight_mouse_cursor"] = "\uf511";
        dict["HighlightMouseCursor"] = "\uf511";
        dict["highlight_off"] = "\ue888";
        dict["HighlightOff"] = "\ue888";
        dict["highlight_text_cursor"] = "\uf512";
        dict["HighlightTextCursor"] = "\uf512";
        dict["highlighter_size_1"] = "\uf76b";
        dict["HighlighterSize1"] = "\uf76b";
        dict["highlighter_size_2"] = "\uf76a";
        dict["HighlighterSize2"] = "\uf76a";
        dict["highlighter_size_3"] = "\uf769";
        dict["HighlighterSize3"] = "\uf769";
        dict["highlighter_size_4"] = "\uf768";
        dict["HighlighterSize4"] = "\uf768";
        dict["highlighter_size_5"] = "\uf767";
        dict["HighlighterSize5"] = "\uf767";
        dict["hiking"] = "\ue50a";
        dict["history"] = "\ue8b3";
        dict["history_2"] = "\uf3e6";
        dict["History2"] = "\uf3e6";
        dict["history_edu"] = "\uea3e";
        dict["HistoryEdu"] = "\uea3e";
        dict["history_off"] = "\uf4da";
        dict["HistoryOff"] = "\uf4da";
        dict["history_toggle_off"] = "\uf17d";
        dict["HistoryToggleOff"] = "\uf17d";
        dict["hive"] = "\ueaa6";
        dict["hls"] = "\ueb8a";
        dict["hls_off"] = "\ueb8c";
        dict["HlsOff"] = "\ueb8c";
        dict["holiday_village"] = "\ue58a";
        dict["HolidayVillage"] = "\ue58a";
        dict["home"] = "\ue9b2";
        dict["home_and_garden"] = "\uef9f";
        dict["HomeAndGarden"] = "\uef9f";
        dict["home_app_logo"] = "\ue295";
        dict["HomeAppLogo"] = "\ue295";
        dict["home_filled"] = "\ue9b2";
        dict["HomeFilled"] = "\ue9b2";
        dict["home_health"] = "\ue4b9";
        dict["HomeHealth"] = "\ue4b9";
        dict["home_improvement_and_tools"] = "\uefa0";
        dict["HomeImprovementAndTools"] = "\uefa0";
        dict["home_iot_device"] = "\ue283";
        dict["HomeIotDevice"] = "\ue283";
        dict["home_max"] = "\uf024";
        dict["HomeMax"] = "\uf024";
        dict["home_max_dots"] = "\ue849";
        dict["HomeMaxDots"] = "\ue849";
        dict["home_mini"] = "\uf025";
        dict["HomeMini"] = "\uf025";
        dict["home_pin"] = "\uf14d";
        dict["HomePin"] = "\uf14d";
        dict["home_repair_service"] = "\uf100";
        dict["HomeRepairService"] = "\uf100";
        dict["home_speaker"] = "\uf11c";
        dict["HomeSpeaker"] = "\uf11c";
        dict["home_storage"] = "\uf86c";
        dict["HomeStorage"] = "\uf86c";
        dict["home_storage_gear"] = "\U000FFF7E";
        dict["HomeStorageGear"] = "\U000FFF7E";
        dict["home_work"] = "\uf030";
        dict["HomeWork"] = "\uf030";
        dict["horizontal_align_center"] = "\U000FFF9C";
        dict["HorizontalAlignCenter"] = "\U000FFF9C";
        dict["horizontal_align_left"] = "\U000FFF9B";
        dict["HorizontalAlignLeft"] = "\U000FFF9B";
        dict["horizontal_align_right"] = "\U000FFF9A";
        dict["HorizontalAlignRight"] = "\U000FFF9A";
        dict["horizontal_distribute"] = "\ue014";
        dict["HorizontalDistribute"] = "\ue014";
        dict["horizontal_rule"] = "\uf108";
        dict["HorizontalRule"] = "\uf108";
        dict["horizontal_split"] = "\ue947";
        dict["HorizontalSplit"] = "\ue947";
        dict["host"] = "\uf3d9";
        dict["hot_tub"] = "\ueb46";
        dict["HotTub"] = "\ueb46";
        dict["hotel"] = "\ue549";
        dict["hotel_class"] = "\ue743";
        dict["HotelClass"] = "\ue743";
        dict["hourglass"] = "\uebff";
        dict["hourglass_arrow_down"] = "\uf37e";
        dict["HourglassArrowDown"] = "\uf37e";
        dict["hourglass_arrow_up"] = "\uf37d";
        dict["HourglassArrowUp"] = "\uf37d";
        dict["hourglass_bottom"] = "\uea5c";
        dict["HourglassBottom"] = "\uea5c";
        dict["hourglass_check"] = "\U000FFFED";
        dict["HourglassCheck"] = "\U000FFFED";
        dict["hourglass_disabled"] = "\uef53";
        dict["HourglassDisabled"] = "\uef53";
        dict["hourglass_empty"] = "\ue88b";
        dict["HourglassEmpty"] = "\ue88b";
        dict["hourglass_full"] = "\ue88c";
        dict["HourglassFull"] = "\ue88c";
        dict["hourglass_pause"] = "\uf38c";
        dict["HourglassPause"] = "\uf38c";
        dict["hourglass_top"] = "\uea5b";
        dict["HourglassTop"] = "\uea5b";
        dict["house"] = "\uea44";
        dict["house_siding"] = "\uf202";
        dict["HouseSiding"] = "\uf202";
        dict["house_with_shield"] = "\ue786";
        dict["HouseWithShield"] = "\ue786";
        dict["houseboat"] = "\ue584";
        dict["household_supplies"] = "\uefa1";
        dict["HouseholdSupplies"] = "\uefa1";
        dict["hov"] = "\uf475";
        dict["how_to_reg"] = "\ue174";
        dict["HowToReg"] = "\ue174";
        dict["how_to_vote"] = "\ue175";
        dict["HowToVote"] = "\ue175";
        dict["hr_resting"] = "\uf6ba";
        dict["HrResting"] = "\uf6ba";
        dict["html"] = "\ueb7e";
        dict["http"] = "\ue902";
        dict["https"] = "\ue899";
        dict["hub"] = "\ue9f4";
        dict["humerus"] = "\uf895";
        dict["humerus_alt"] = "\uf896";
        dict["HumerusAlt"] = "\uf896";
        dict["humidity_high"] = "\uf163";
        dict["HumidityHigh"] = "\uf163";
        dict["humidity_indoor"] = "\uf558";
        dict["HumidityIndoor"] = "\uf558";
        dict["humidity_low"] = "\uf164";
        dict["HumidityLow"] = "\uf164";
        dict["humidity_mid"] = "\uf165";
        dict["HumidityMid"] = "\uf165";
        dict["humidity_percentage"] = "\uf87e";
        dict["HumidityPercentage"] = "\uf87e";
        dict["hvac"] = "\uf10e";
        dict["hvac_max_defrost"] = "\uf332";
        dict["HvacMaxDefrost"] = "\uf332";
        dict["ice_skating"] = "\ue50b";
        dict["IceSkating"] = "\ue50b";
        dict["icecream"] = "\uea69";
        dict["id_card"] = "\uf4ca";
        dict["IdCard"] = "\uf4ca";
        dict["id_card_2"] = "\U000FFEEA";
        dict["IdCard2"] = "\U000FFEEA";
        dict["identity_aware_proxy"] = "\ue2dd";
        dict["IdentityAwareProxy"] = "\ue2dd";
        dict["identity_platform"] = "\uebb7";
        dict["IdentityPlatform"] = "\uebb7";
        dict["ifl"] = "\ue025";
        dict["iframe"] = "\uf71b";
        dict["iframe_off"] = "\uf71c";
        dict["IframeOff"] = "\uf71c";
        dict["image"] = "\ue3f4";
        dict["image_arrow_up"] = "\uf317";
        dict["ImageArrowUp"] = "\uf317";
        dict["image_aspect_ratio"] = "\ue6a6";
        dict["ImageAspectRatio"] = "\ue6a6";
        dict["image_inset"] = "\uf247";
        dict["ImageInset"] = "\uf247";
        dict["image_not_supported"] = "\uf116";
        dict["ImageNotSupported"] = "\uf116";
        dict["image_search"] = "\ue43f";
        dict["ImageSearch"] = "\ue43f";
        dict["imagesearch_roller"] = "\ue9b4";
        dict["ImagesearchRoller"] = "\ue9b4";
        dict["imagesmode"] = "\uefa2";
        dict["immunology"] = "\ue0fb";
        dict["import_contacts"] = "\ue0e0";
        dict["ImportContacts"] = "\ue0e0";
        dict["import_export"] = "\ue8d5";
        dict["ImportExport"] = "\ue8d5";
        dict["important_devices"] = "\ue912";
        dict["ImportantDevices"] = "\ue912";
        dict["in_home_mode"] = "\ue833";
        dict["InHomeMode"] = "\ue833";
        dict["inactive_order"] = "\ue0fc";
        dict["InactiveOrder"] = "\ue0fc";
        dict["inbox"] = "\ue156";
        dict["inbox_customize"] = "\uf859";
        dict["InboxCustomize"] = "\uf859";
        dict["inbox_text"] = "\uf399";
        dict["InboxText"] = "\uf399";
        dict["inbox_text_asterisk"] = "\uf360";
        dict["InboxTextAsterisk"] = "\uf360";
        dict["inbox_text_person"] = "\uf35e";
        dict["InboxTextPerson"] = "\uf35e";
        dict["inbox_text_share"] = "\uf35c";
        dict["InboxTextShare"] = "\uf35c";
        dict["incomplete_circle"] = "\ue79b";
        dict["IncompleteCircle"] = "\ue79b";
        dict["indeterminate_check_box"] = "\ue909";
        dict["IndeterminateCheckBox"] = "\ue909";
        dict["indeterminate_question_box"] = "\uf56d";
        dict["IndeterminateQuestionBox"] = "\uf56d";
        dict["info"] = "\ue88e";
        dict["info_i"] = "\uf59b";
        dict["InfoI"] = "\uf59b";
        dict["infrared"] = "\uf87c";
        dict["ink_eraser"] = "\ue6d0";
        dict["InkEraser"] = "\ue6d0";
        dict["ink_eraser_off"] = "\ue7e3";
        dict["InkEraserOff"] = "\ue7e3";
        dict["ink_highlighter"] = "\ue6d1";
        dict["InkHighlighter"] = "\ue6d1";
        dict["ink_highlighter_move"] = "\uf524";
        dict["InkHighlighterMove"] = "\uf524";
        dict["ink_highlighter_off"] = "\U000FFF14";
        dict["InkHighlighterOff"] = "\U000FFF14";
        dict["ink_marker"] = "\ue6d2";
        dict["InkMarker"] = "\ue6d2";
        dict["ink_pen"] = "\ue6d3";
        dict["InkPen"] = "\ue6d3";
        dict["ink_selection"] = "\uef52";
        dict["InkSelection"] = "\uef52";
        dict["inpatient"] = "\ue0fe";
        dict["input"] = "\ue890";
        dict["input_circle"] = "\uf71a";
        dict["InputCircle"] = "\uf71a";
        dict["insert_chart"] = "\uf0cc";
        dict["InsertChart"] = "\uf0cc";
        dict["insert_chart_filled"] = "\uf0cc";
        dict["InsertChartFilled"] = "\uf0cc";
        dict["insert_chart_outlined"] = "\uf0cc";
        dict["InsertChartOutlined"] = "\uf0cc";
        dict["insert_comment"] = "\ue24c";
        dict["InsertComment"] = "\ue24c";
        dict["insert_drive_file"] = "\ue66d";
        dict["InsertDriveFile"] = "\ue66d";
        dict["insert_emoticon"] = "\uea22";
        dict["InsertEmoticon"] = "\uea22";
        dict["insert_invitation"] = "\ue878";
        dict["InsertInvitation"] = "\ue878";
        dict["insert_link"] = "\ue250";
        dict["InsertLink"] = "\ue250";
        dict["insert_page_break"] = "\ueaca";
        dict["InsertPageBreak"] = "\ueaca";
        dict["insert_photo"] = "\ue3f4";
        dict["InsertPhoto"] = "\ue3f4";
        dict["insert_text"] = "\uf827";
        dict["InsertText"] = "\uf827";
        dict["insights"] = "\uf092";
        dict["install_desktop"] = "\ueb71";
        dict["InstallDesktop"] = "\ueb71";
        dict["install_mobile"] = "\uf2cd";
        dict["InstallMobile"] = "\uf2cd";
        dict["instant_mix"] = "\ue026";
        dict["InstantMix"] = "\ue026";
        dict["integration_instructions"] = "\uef54";
        dict["IntegrationInstructions"] = "\uef54";
        dict["interactive_space"] = "\uf7ff";
        dict["InteractiveSpace"] = "\uf7ff";
        dict["interests"] = "\ue7c8";
        dict["interpreter_mode"] = "\ue83b";
        dict["InterpreterMode"] = "\ue83b";
        dict["inventory"] = "\ue179";
        dict["inventory_2"] = "\ue1a1";
        dict["Inventory2"] = "\ue1a1";
        dict["invert_colors"] = "\ue891";
        dict["InvertColors"] = "\ue891";
        dict["invert_colors_off"] = "\ue0c4";
        dict["InvertColorsOff"] = "\ue0c4";
        dict["ios"] = "\ue027";
        dict["ios_share"] = "\ue6b8";
        dict["IosShare"] = "\ue6b8";
        dict["iron"] = "\ue583";
        dict["iso"] = "\ue3f6";
        dict["jamboard_kiosk"] = "\ue9b5";
        dict["JamboardKiosk"] = "\ue9b5";
        dict["japanese_curry"] = "\uf284";
        dict["JapaneseCurry"] = "\uf284";
        dict["japanese_flag"] = "\uf283";
        dict["JapaneseFlag"] = "\uf283";
        dict["javascript"] = "\ueb7c";
        dict["jewelry"] = "\U000FFEDB";
        dict["join"] = "\uf84f";
        dict["join_full"] = "\uf84f";
        dict["JoinFull"] = "\uf84f";
        dict["join_inner"] = "\ueaf4";
        dict["JoinInner"] = "\ueaf4";
        dict["join_left"] = "\ueaf2";
        dict["JoinLeft"] = "\ueaf2";
        dict["join_right"] = "\ueaea";
        dict["JoinRight"] = "\ueaea";
        dict["joystick"] = "\uf5ee";
        dict["jump_to_element"] = "\uf719";
        dict["JumpToElement"] = "\uf719";
        dict["kanji_alcohol"] = "\uf23e";
        dict["KanjiAlcohol"] = "\uf23e";
        dict["kayaking"] = "\ue50c";
        dict["kebab_dining"] = "\ue842";
        dict["KebabDining"] = "\ue842";
        dict["keep"] = "\uf027";
        dict["keep_off"] = "\ue6f9";
        dict["KeepOff"] = "\ue6f9";
        dict["keep_pin"] = "\uf027";
        dict["KeepPin"] = "\uf027";
        dict["keep_public"] = "\uf56f";
        dict["KeepPublic"] = "\uf56f";
        dict["kettle"] = "\ue2b9";
        dict["key"] = "\ue73c";
        dict["key_off"] = "\ueb84";
        dict["KeyOff"] = "\ueb84";
        dict["key_vertical"] = "\uf51a";
        dict["KeyVertical"] = "\uf51a";
        dict["key_visualizer"] = "\uf199";
        dict["KeyVisualizer"] = "\uf199";
        dict["keyboard"] = "\ue312";
        dict["keyboard_alt"] = "\uf028";
        dict["KeyboardAlt"] = "\uf028";
        dict["keyboard_arrow_down"] = "\ue313";
        dict["KeyboardArrowDown"] = "\ue313";
        dict["keyboard_arrow_left"] = "\ue314";
        dict["KeyboardArrowLeft"] = "\ue314";
        dict["keyboard_arrow_right"] = "\ue315";
        dict["KeyboardArrowRight"] = "\ue315";
        dict["keyboard_arrow_up"] = "\ue316";
        dict["KeyboardArrowUp"] = "\ue316";
        dict["keyboard_backspace"] = "\ue317";
        dict["KeyboardBackspace"] = "\ue317";
        dict["keyboard_capslock"] = "\ue318";
        dict["KeyboardCapslock"] = "\ue318";
        dict["keyboard_capslock_badge"] = "\uf7de";
        dict["KeyboardCapslockBadge"] = "\uf7de";
        dict["keyboard_command_key"] = "\ueae7";
        dict["KeyboardCommandKey"] = "\ueae7";
        dict["keyboard_control_key"] = "\ueae6";
        dict["KeyboardControlKey"] = "\ueae6";
        dict["keyboard_double_arrow_down"] = "\uead0";
        dict["KeyboardDoubleArrowDown"] = "\uead0";
        dict["keyboard_double_arrow_left"] = "\ueac3";
        dict["KeyboardDoubleArrowLeft"] = "\ueac3";
        dict["keyboard_double_arrow_right"] = "\ueac9";
        dict["KeyboardDoubleArrowRight"] = "\ueac9";
        dict["keyboard_double_arrow_up"] = "\ueacf";
        dict["KeyboardDoubleArrowUp"] = "\ueacf";
        dict["keyboard_external_input"] = "\uf7dd";
        dict["KeyboardExternalInput"] = "\uf7dd";
        dict["keyboard_full"] = "\uf7dc";
        dict["KeyboardFull"] = "\uf7dc";
        dict["keyboard_hide"] = "\ue31a";
        dict["KeyboardHide"] = "\ue31a";
        dict["keyboard_keys"] = "\uf67b";
        dict["KeyboardKeys"] = "\uf67b";
        dict["keyboard_lock"] = "\uf492";
        dict["KeyboardLock"] = "\uf492";
        dict["keyboard_lock_off"] = "\uf491";
        dict["KeyboardLockOff"] = "\uf491";
        dict["keyboard_off"] = "\uf67a";
        dict["KeyboardOff"] = "\uf67a";
        dict["keyboard_onscreen"] = "\uf7db";
        dict["KeyboardOnscreen"] = "\uf7db";
        dict["keyboard_option_key"] = "\ueae8";
        dict["KeyboardOptionKey"] = "\ueae8";
        dict["keyboard_previous_language"] = "\uf7da";
        dict["KeyboardPreviousLanguage"] = "\uf7da";
        dict["keyboard_return"] = "\ue31b";
        dict["KeyboardReturn"] = "\ue31b";
        dict["keyboard_tab"] = "\ue31c";
        dict["KeyboardTab"] = "\ue31c";
        dict["keyboard_tab_rtl"] = "\uec73";
        dict["KeyboardTabRtl"] = "\uec73";
        dict["keyboard_voice"] = "\ue31d";
        dict["KeyboardVoice"] = "\ue31d";
        dict["kid_star"] = "\uf526";
        dict["KidStar"] = "\uf526";
        dict["king_bed"] = "\uea45";
        dict["KingBed"] = "\uea45";
        dict["kitchen"] = "\ueb47";
        dict["kitesurfing"] = "\ue50d";
        dict["lab_panel"] = "\ue103";
        dict["LabPanel"] = "\ue103";
        dict["lab_profile"] = "\ue104";
        dict["LabProfile"] = "\ue104";
        dict["lab_research"] = "\uf80b";
        dict["LabResearch"] = "\uf80b";
        dict["label"] = "\ue893";
        dict["label_important"] = "\ue948";
        dict["LabelImportant"] = "\ue948";
        dict["label_important_outline"] = "\ue948";
        dict["LabelImportantOutline"] = "\ue948";
        dict["label_off"] = "\ue9b6";
        dict["LabelOff"] = "\ue9b6";
        dict["label_outline"] = "\ue893";
        dict["LabelOutline"] = "\ue893";
        dict["labs"] = "\ue105";
        dict["lan"] = "\ueb2f";
        dict["landscape"] = "\ue564";
        dict["landscape_2"] = "\uf4c4";
        dict["Landscape2"] = "\uf4c4";
        dict["landscape_2_edit"] = "\uf310";
        dict["Landscape2Edit"] = "\uf310";
        dict["landscape_2_off"] = "\uf4c3";
        dict["Landscape2Off"] = "\uf4c3";
        dict["landslide"] = "\uebd7";
        dict["language"] = "\uea07";
        dict["language_chinese_array"] = "\uf766";
        dict["LanguageChineseArray"] = "\uf766";
        dict["language_chinese_cangjie"] = "\uf765";
        dict["LanguageChineseCangjie"] = "\uf765";
        dict["language_chinese_dayi"] = "\uf764";
        dict["LanguageChineseDayi"] = "\uf764";
        dict["language_chinese_pinyin"] = "\uf763";
        dict["LanguageChinesePinyin"] = "\uf763";
        dict["language_chinese_quick"] = "\uf762";
        dict["LanguageChineseQuick"] = "\uf762";
        dict["language_chinese_wubi"] = "\uf761";
        dict["LanguageChineseWubi"] = "\uf761";
        dict["language_french"] = "\uf760";
        dict["LanguageFrench"] = "\uf760";
        dict["language_gb_english"] = "\uf75f";
        dict["LanguageGbEnglish"] = "\uf75f";
        dict["language_international"] = "\uf75e";
        dict["LanguageInternational"] = "\uf75e";
        dict["language_japanese_kana"] = "\uf513";
        dict["LanguageJapaneseKana"] = "\uf513";
        dict["language_korean_latin"] = "\uf75d";
        dict["LanguageKoreanLatin"] = "\uf75d";
        dict["language_pinyin"] = "\uf75c";
        dict["LanguagePinyin"] = "\uf75c";
        dict["language_spanish"] = "\uf5e9";
        dict["LanguageSpanish"] = "\uf5e9";
        dict["language_us"] = "\uf759";
        dict["LanguageUs"] = "\uf759";
        dict["language_us_colemak"] = "\uf75b";
        dict["LanguageUsColemak"] = "\uf75b";
        dict["language_us_dvorak"] = "\uf75a";
        dict["LanguageUsDvorak"] = "\uf75a";
        dict["laps"] = "\uf6b9";
        dict["laptop"] = "\ue31e";
        dict["laptop_car"] = "\uf3cd";
        dict["LaptopCar"] = "\uf3cd";
        dict["laptop_chromebook"] = "\ue31f";
        dict["LaptopChromebook"] = "\ue31f";
        dict["laptop_mac"] = "\ue320";
        dict["LaptopMac"] = "\ue320";
        dict["laptop_windows"] = "\ue321";
        dict["LaptopWindows"] = "\ue321";
        dict["lasso_select"] = "\ueb03";
        dict["LassoSelect"] = "\ueb03";
        dict["last_page"] = "\ue5dd";
        dict["LastPage"] = "\ue5dd";
        dict["launch"] = "\ue89e";
        dict["laundry"] = "\ue2a8";
        dict["layers"] = "\ue53b";
        dict["layers_clear"] = "\ue53c";
        dict["LayersClear"] = "\ue53c";
        dict["lda"] = "\ue106";
        dict["leaderboard"] = "\uf20c";
        dict["leak_add"] = "\ue3f8";
        dict["LeakAdd"] = "\ue3f8";
        dict["leak_remove"] = "\ue3f9";
        dict["LeakRemove"] = "\ue3f9";
        dict["left_click"] = "\uf718";
        dict["LeftClick"] = "\uf718";
        dict["left_panel_close"] = "\uf717";
        dict["LeftPanelClose"] = "\uf717";
        dict["left_panel_open"] = "\uf716";
        dict["LeftPanelOpen"] = "\uf716";
        dict["legend_toggle"] = "\uf11b";
        dict["LegendToggle"] = "\uf11b";
        dict["lens"] = "\ue3fa";
        dict["lens_blur"] = "\uf029";
        dict["LensBlur"] = "\uf029";
        dict["letter_switch"] = "\uf758";
        dict["LetterSwitch"] = "\uf758";
        dict["library_add"] = "\ue03c";
        dict["LibraryAdd"] = "\ue03c";
        dict["library_add_check"] = "\ue9b7";
        dict["LibraryAddCheck"] = "\ue9b7";
        dict["library_books"] = "\ue02f";
        dict["LibraryBooks"] = "\ue02f";
        dict["library_music"] = "\ue030";
        dict["LibraryMusic"] = "\ue030";
        dict["license"] = "\ueb04";
        dict["lift_to_talk"] = "\uefa3";
        dict["LiftToTalk"] = "\uefa3";
        dict["light"] = "\uf02a";
        dict["light_group"] = "\ue28b";
        dict["LightGroup"] = "\ue28b";
        dict["light_group_2"] = "\U000FFF76";
        dict["LightGroup2"] = "\U000FFF76";
        dict["light_mode"] = "\ue518";
        dict["LightMode"] = "\ue518";
        dict["light_mode_auto"] = "\U000FFF00";
        dict["LightModeAuto"] = "\U000FFF00";
        dict["light_off"] = "\ue9b8";
        dict["LightOff"] = "\ue9b8";
        dict["lightbulb"] = "\ue90f";
        dict["lightbulb_2"] = "\uf3e3";
        dict["Lightbulb2"] = "\uf3e3";
        dict["lightbulb_circle"] = "\uebfe";
        dict["LightbulbCircle"] = "\uebfe";
        dict["lightbulb_outline"] = "\ue90f";
        dict["LightbulbOutline"] = "\ue90f";
        dict["lightning_stand"] = "\uefa4";
        dict["LightningStand"] = "\uefa4";
        dict["lightstrip"] = "\U000FFF75";
        dict["line_axis"] = "\uea9a";
        dict["LineAxis"] = "\uea9a";
        dict["line_curve"] = "\uf757";
        dict["LineCurve"] = "\uf757";
        dict["line_end"] = "\uf826";
        dict["LineEnd"] = "\uf826";
        dict["line_end_arrow"] = "\uf81d";
        dict["LineEndArrow"] = "\uf81d";
        dict["line_end_arrow_notch"] = "\uf81c";
        dict["LineEndArrowNotch"] = "\uf81c";
        dict["line_end_circle"] = "\uf81b";
        dict["LineEndCircle"] = "\uf81b";
        dict["line_end_diamond"] = "\uf81a";
        dict["LineEndDiamond"] = "\uf81a";
        dict["line_end_square"] = "\uf819";
        dict["LineEndSquare"] = "\uf819";
        dict["line_start"] = "\uf825";
        dict["LineStart"] = "\uf825";
        dict["line_start_arrow"] = "\uf818";
        dict["LineStartArrow"] = "\uf818";
        dict["line_start_arrow_notch"] = "\uf817";
        dict["LineStartArrowNotch"] = "\uf817";
        dict["line_start_circle"] = "\uf816";
        dict["LineStartCircle"] = "\uf816";
        dict["line_start_diamond"] = "\uf815";
        dict["LineStartDiamond"] = "\uf815";
        dict["line_start_square"] = "\uf814";
        dict["LineStartSquare"] = "\uf814";
        dict["line_style"] = "\ue919";
        dict["LineStyle"] = "\ue919";
        dict["line_weight"] = "\ue91a";
        dict["LineWeight"] = "\ue91a";
        dict["linear_scale"] = "\ue260";
        dict["LinearScale"] = "\ue260";
        dict["link"] = "\ue250";
        dict["link_2"] = "\U000FFFB5";
        dict["Link2"] = "\U000FFFB5";
        dict["link_off"] = "\ue16f";
        dict["LinkOff"] = "\ue16f";
        dict["linked_camera"] = "\ue438";
        dict["LinkedCamera"] = "\ue438";
        dict["linked_services"] = "\uf535";
        dict["LinkedServices"] = "\uf535";
        dict["lips"] = "\ueeb2";
        dict["liquor"] = "\uea60";
        dict["list"] = "\ue896";
        dict["list_2"] = "\U000FFECA";
        dict["List2"] = "\U000FFECA";
        dict["list_alt"] = "\ue0ee";
        dict["ListAlt"] = "\ue0ee";
        dict["list_alt_add"] = "\uf756";
        dict["ListAltAdd"] = "\uf756";
        dict["list_alt_check"] = "\uf3de";
        dict["ListAltCheck"] = "\uf3de";
        dict["list_arrow"] = "\U000FFF33";
        dict["ListArrow"] = "\U000FFF33";
        dict["lists"] = "\ue9b9";
        dict["live_help"] = "\ue0c6";
        dict["LiveHelp"] = "\ue0c6";
        dict["live_tv"] = "\ue63a";
        dict["LiveTv"] = "\ue63a";
        dict["living"] = "\uf02b";
        dict["local_activity"] = "\ue553";
        dict["LocalActivity"] = "\ue553";
        dict["local_airport"] = "\ue53d";
        dict["LocalAirport"] = "\ue53d";
        dict["local_atm"] = "\ue53e";
        dict["LocalAtm"] = "\ue53e";
        dict["local_bar"] = "\ue540";
        dict["LocalBar"] = "\ue540";
        dict["local_cafe"] = "\ueb44";
        dict["LocalCafe"] = "\ueb44";
        dict["local_car_wash"] = "\ue542";
        dict["LocalCarWash"] = "\ue542";
        dict["local_convenience_store"] = "\ue543";
        dict["LocalConvenienceStore"] = "\ue543";
        dict["local_dining"] = "\ue561";
        dict["LocalDining"] = "\ue561";
        dict["local_drink"] = "\ue544";
        dict["LocalDrink"] = "\ue544";
        dict["local_fire_department"] = "\uef55";
        dict["LocalFireDepartment"] = "\uef55";
        dict["local_florist"] = "\ue545";
        dict["LocalFlorist"] = "\ue545";
        dict["local_gas_station"] = "\ue546";
        dict["LocalGasStation"] = "\ue546";
        dict["local_grocery_store"] = "\ue8cc";
        dict["LocalGroceryStore"] = "\ue8cc";
        dict["local_hospital"] = "\ue548";
        dict["LocalHospital"] = "\ue548";
        dict["local_hotel"] = "\ue549";
        dict["LocalHotel"] = "\ue549";
        dict["local_laundry_service"] = "\ue54a";
        dict["LocalLaundryService"] = "\ue54a";
        dict["local_library"] = "\ue54b";
        dict["LocalLibrary"] = "\ue54b";
        dict["local_mall"] = "\ue54c";
        dict["LocalMall"] = "\ue54c";
        dict["local_movies"] = "\ue8da";
        dict["LocalMovies"] = "\ue8da";
        dict["local_offer"] = "\uf05b";
        dict["LocalOffer"] = "\uf05b";
        dict["local_parking"] = "\ue54f";
        dict["LocalParking"] = "\ue54f";
        dict["local_pharmacy"] = "\ue550";
        dict["LocalPharmacy"] = "\ue550";
        dict["local_phone"] = "\uf0d4";
        dict["LocalPhone"] = "\uf0d4";
        dict["local_pizza"] = "\ue552";
        dict["LocalPizza"] = "\ue552";
        dict["local_play"] = "\ue553";
        dict["LocalPlay"] = "\ue553";
        dict["local_police"] = "\uef56";
        dict["LocalPolice"] = "\uef56";
        dict["local_post_office"] = "\ue554";
        dict["LocalPostOffice"] = "\ue554";
        dict["local_printshop"] = "\ue8ad";
        dict["LocalPrintshop"] = "\ue8ad";
        dict["local_see"] = "\ue557";
        dict["LocalSee"] = "\ue557";
        dict["local_shipping"] = "\ue558";
        dict["LocalShipping"] = "\ue558";
        dict["local_taxi"] = "\ue559";
        dict["LocalTaxi"] = "\ue559";
        dict["location_automation"] = "\uf14f";
        dict["LocationAutomation"] = "\uf14f";
        dict["location_away"] = "\uf150";
        dict["LocationAway"] = "\uf150";
        dict["location_chip"] = "\uf850";
        dict["LocationChip"] = "\uf850";
        dict["location_city"] = "\ue7f1";
        dict["LocationCity"] = "\ue7f1";
        dict["location_disabled"] = "\ue1b6";
        dict["LocationDisabled"] = "\ue1b6";
        dict["location_home"] = "\uf152";
        dict["LocationHome"] = "\uf152";
        dict["location_off"] = "\ue0c7";
        dict["LocationOff"] = "\ue0c7";
        dict["location_on"] = "\uf1db";
        dict["LocationOn"] = "\uf1db";
        dict["location_pin"] = "\uf1db";
        dict["LocationPin"] = "\uf1db";
        dict["location_searching"] = "\ue1b7";
        dict["LocationSearching"] = "\ue1b7";
        dict["locator_tag"] = "\uf8c1";
        dict["LocatorTag"] = "\uf8c1";
        dict["lock"] = "\ue899";
        dict["lock_clock"] = "\uef57";
        dict["LockClock"] = "\uef57";
        dict["lock_open"] = "\ue898";
        dict["LockOpen"] = "\ue898";
        dict["lock_open_circle"] = "\uf361";
        dict["LockOpenCircle"] = "\uf361";
        dict["lock_open_right"] = "\uf656";
        dict["LockOpenRight"] = "\uf656";
        dict["lock_outline"] = "\ue899";
        dict["LockOutline"] = "\ue899";
        dict["lock_person"] = "\uf8f3";
        dict["LockPerson"] = "\uf8f3";
        dict["lock_reset"] = "\ueade";
        dict["LockReset"] = "\ueade";
        dict["login"] = "\uea77";
        dict["logo_dev"] = "\uead6";
        dict["LogoDev"] = "\uead6";
        dict["logout"] = "\ue9ba";
        dict["looks"] = "\ue3fc";
        dict["looks_3"] = "\ue3fb";
        dict["Looks3"] = "\ue3fb";
        dict["looks_4"] = "\ue3fd";
        dict["Looks4"] = "\ue3fd";
        dict["looks_5"] = "\ue3fe";
        dict["Looks5"] = "\ue3fe";
        dict["looks_6"] = "\ue3ff";
        dict["Looks6"] = "\ue3ff";
        dict["looks_one"] = "\ue400";
        dict["LooksOne"] = "\ue400";
        dict["looks_two"] = "\ue401";
        dict["LooksTwo"] = "\ue401";
        dict["loop"] = "\ue863";
        dict["loupe"] = "\ue402";
        dict["low_density"] = "\uf79b";
        dict["LowDensity"] = "\uf79b";
        dict["low_priority"] = "\ue16d";
        dict["LowPriority"] = "\ue16d";
        dict["lowercase"] = "\uf48a";
        dict["loyalty"] = "\ue89a";
        dict["lte_mobiledata"] = "\uf02c";
        dict["LteMobiledata"] = "\uf02c";
        dict["lte_mobiledata_badge"] = "\uf7d9";
        dict["LteMobiledataBadge"] = "\uf7d9";
        dict["lte_plus_mobiledata"] = "\uf02d";
        dict["LtePlusMobiledata"] = "\uf02d";
        dict["lte_plus_mobiledata_badge"] = "\uf7d8";
        dict["LtePlusMobiledataBadge"] = "\uf7d8";
        dict["luggage"] = "\uf235";
        dict["lunch_dining"] = "\uea61";
        dict["LunchDining"] = "\uea61";
        dict["lyrics"] = "\uec0b";
        dict["macro_auto"] = "\uf6f2";
        dict["MacroAuto"] = "\uf6f2";
        dict["macro_off"] = "\uf8d2";
        dict["MacroOff"] = "\uf8d2";
        dict["magic_button"] = "\uf136";
        dict["MagicButton"] = "\uf136";
        dict["magic_exchange"] = "\uf7f4";
        dict["MagicExchange"] = "\uf7f4";
        dict["magic_tether"] = "\uf7d7";
        dict["MagicTether"] = "\uf7d7";
        dict["magnification_large"] = "\uf83d";
        dict["MagnificationLarge"] = "\uf83d";
        dict["magnification_small"] = "\uf83c";
        dict["MagnificationSmall"] = "\uf83c";
        dict["magnify_docked"] = "\uf7d6";
        dict["MagnifyDocked"] = "\uf7d6";
        dict["magnify_fullscreen"] = "\uf7d5";
        dict["MagnifyFullscreen"] = "\uf7d5";
        dict["mail"] = "\ue159";
        dict["mail_asterisk"] = "\ueef4";
        dict["MailAsterisk"] = "\ueef4";
        dict["mail_lock"] = "\uec0a";
        dict["MailLock"] = "\uec0a";
        dict["mail_off"] = "\uf48b";
        dict["MailOff"] = "\uf48b";
        dict["mail_outline"] = "\ue159";
        dict["MailOutline"] = "\ue159";
        dict["mail_shield"] = "\uf249";
        dict["MailShield"] = "\uf249";
        dict["male"] = "\ue58e";
        dict["man"] = "\ue4eb";
        dict["man_2"] = "\uf8e1";
        dict["Man2"] = "\uf8e1";
        dict["man_3"] = "\uf8e2";
        dict["Man3"] = "\uf8e2";
        dict["man_4"] = "\uf8e3";
        dict["Man4"] = "\uf8e3";
        dict["manage_accounts"] = "\uf02e";
        dict["ManageAccounts"] = "\uf02e";
        dict["manage_history"] = "\uebe7";
        dict["ManageHistory"] = "\uebe7";
        dict["manage_search"] = "\uf02f";
        dict["ManageSearch"] = "\uf02f";
        dict["manga"] = "\uf5e3";
        dict["manufacturing"] = "\ue726";
        dict["map"] = "\ue55b";
        dict["map_pin_heart"] = "\uf298";
        dict["MapPinHeart"] = "\uf298";
        dict["map_pin_review"] = "\uf297";
        dict["MapPinReview"] = "\uf297";
        dict["map_search"] = "\uf3ca";
        dict["MapSearch"] = "\uf3ca";
        dict["maps_home_work"] = "\uf030";
        dict["MapsHomeWork"] = "\uf030";
        dict["maps_ugc"] = "\uef58";
        dict["MapsUgc"] = "\uef58";
        dict["margin"] = "\ue9bb";
        dict["mark_as_unread"] = "\ue9bc";
        dict["MarkAsUnread"] = "\ue9bc";
        dict["mark_chat_read"] = "\uf18b";
        dict["MarkChatRead"] = "\uf18b";
        dict["mark_chat_unread"] = "\uf189";
        dict["MarkChatUnread"] = "\uf189";
        dict["mark_email_read"] = "\uf18c";
        dict["MarkEmailRead"] = "\uf18c";
        dict["mark_email_unread"] = "\uf18a";
        dict["MarkEmailUnread"] = "\uf18a";
        dict["mark_unread_chat_alt"] = "\ueb9d";
        dict["MarkUnreadChatAlt"] = "\ueb9d";
        dict["markdown"] = "\uf552";
        dict["markdown_copy"] = "\uf553";
        dict["MarkdownCopy"] = "\uf553";
        dict["markdown_paste"] = "\uf554";
        dict["MarkdownPaste"] = "\uf554";
        dict["markunread"] = "\ue159";
        dict["markunread_mailbox"] = "\ue89b";
        dict["MarkunreadMailbox"] = "\ue89b";
        dict["masked_transitions"] = "\ue72e";
        dict["MaskedTransitions"] = "\ue72e";
        dict["masked_transitions_add"] = "\uf42b";
        dict["MaskedTransitionsAdd"] = "\uf42b";
        dict["masks"] = "\uf218";
        dict["massage"] = "\uf2c2";
        dict["match_case"] = "\uf6f1";
        dict["MatchCase"] = "\uf6f1";
        dict["match_case_off"] = "\uf36f";
        dict["MatchCaseOff"] = "\uf36f";
        dict["match_word"] = "\uf6f0";
        dict["MatchWord"] = "\uf6f0";
        dict["matter"] = "\ue907";
        dict["maximize"] = "\ue930";
        dict["meal_dinner"] = "\uf23d";
        dict["MealDinner"] = "\uf23d";
        dict["meal_lunch"] = "\uf23c";
        dict["MealLunch"] = "\uf23c";
        dict["measuring_tape"] = "\uf6af";
        dict["MeasuringTape"] = "\uf6af";
        dict["media_bluetooth_off"] = "\uf031";
        dict["MediaBluetoothOff"] = "\uf031";
        dict["media_bluetooth_on"] = "\uf032";
        dict["MediaBluetoothOn"] = "\uf032";
        dict["media_link"] = "\uf83f";
        dict["MediaLink"] = "\uf83f";
        dict["media_output"] = "\uf4f2";
        dict["MediaOutput"] = "\uf4f2";
        dict["media_output_off"] = "\uf4f3";
        dict["MediaOutputOff"] = "\uf4f3";
        dict["mediation"] = "\uefa7";
        dict["medical_information"] = "\uebed";
        dict["MedicalInformation"] = "\uebed";
        dict["medical_mask"] = "\uf80a";
        dict["MedicalMask"] = "\uf80a";
        dict["medical_services"] = "\uf109";
        dict["MedicalServices"] = "\uf109";
        dict["medication"] = "\uf033";
        dict["medication_liquid"] = "\uea87";
        dict["MedicationLiquid"] = "\uea87";
        dict["meeting_room"] = "\ueb4f";
        dict["MeetingRoom"] = "\ueb4f";
        dict["memory"] = "\ue322";
        dict["memory_alt"] = "\uf7a3";
        dict["MemoryAlt"] = "\uf7a3";
        dict["menstrual_health"] = "\uf6e1";
        dict["MenstrualHealth"] = "\uf6e1";
        dict["menu"] = "\ue5d2";
        dict["menu_book"] = "\uea19";
        dict["MenuBook"] = "\uea19";
        dict["menu_book_2"] = "\uf291";
        dict["MenuBook2"] = "\uf291";
        dict["menu_open"] = "\ue9bd";
        dict["MenuOpen"] = "\ue9bd";
        dict["merge"] = "\ueb98";
        dict["merge_type"] = "\ue252";
        dict["MergeType"] = "\ue252";
        dict["message"] = "\ue0c9";
        dict["metabolism"] = "\ue10b";
        dict["metro"] = "\uf474";
        dict["mfg_nest_yale_lock"] = "\uf11d";
        dict["MfgNestYaleLock"] = "\uf11d";
        dict["mic"] = "\ue31d";
        dict["mic_alert"] = "\uf392";
        dict["MicAlert"] = "\uf392";
        dict["mic_double"] = "\uf5d1";
        dict["MicDouble"] = "\uf5d1";
        dict["mic_external_off"] = "\uef59";
        dict["MicExternalOff"] = "\uef59";
        dict["mic_external_on"] = "\uef5a";
        dict["MicExternalOn"] = "\uef5a";
        dict["mic_gear"] = "\ueeba";
        dict["MicGear"] = "\ueeba";
        dict["mic_none"] = "\ue31d";
        dict["MicNone"] = "\ue31d";
        dict["mic_off"] = "\ue02b";
        dict["MicOff"] = "\ue02b";
        dict["microbiology"] = "\ue10c";
        dict["microwave"] = "\uf204";
        dict["microwave_gen"] = "\ue847";
        dict["MicrowaveGen"] = "\ue847";
        dict["military_tech"] = "\uea3f";
        dict["MilitaryTech"] = "\uea3f";
        dict["mimo"] = "\ue9be";
        dict["mimo_disconnect"] = "\ue9bf";
        dict["MimoDisconnect"] = "\ue9bf";
        dict["mindfulness"] = "\uf6e0";
        dict["minimize"] = "\ue931";
        dict["minor_crash"] = "\uebf1";
        dict["MinorCrash"] = "\uebf1";
        dict["mintmark"] = "\uefa9";
        dict["missed_video_call"] = "\uf0ce";
        dict["MissedVideoCall"] = "\uf0ce";
        dict["missed_video_call_filled"] = "\uf0ce";
        dict["MissedVideoCallFilled"] = "\uf0ce";
        dict["missing_controller"] = "\ue701";
        dict["MissingController"] = "\ue701";
        dict["mist"] = "\ue188";
        dict["mitre"] = "\uf547";
        dict["mixture_med"] = "\ue4c8";
        dict["MixtureMed"] = "\ue4c8";
        dict["mms"] = "\ue618";
        dict["mobile"] = "\ue7ba";
        dict["mobile_2"] = "\uf2db";
        dict["Mobile2"] = "\uf2db";
        dict["mobile_3"] = "\uf2da";
        dict["Mobile3"] = "\uf2da";
        dict["mobile_alert"] = "\uf2d3";
        dict["MobileAlert"] = "\uf2d3";
        dict["mobile_arrow_down"] = "\uf2cd";
        dict["MobileArrowDown"] = "\uf2cd";
        dict["mobile_arrow_right"] = "\uf2d2";
        dict["MobileArrowRight"] = "\uf2d2";
        dict["mobile_arrow_up_right"] = "\uf2b9";
        dict["MobileArrowUpRight"] = "\uf2b9";
        dict["mobile_block"] = "\uf2e5";
        dict["MobileBlock"] = "\uf2e5";
        dict["mobile_camera"] = "\uf44e";
        dict["MobileCamera"] = "\uf44e";
        dict["mobile_camera_front"] = "\uf2c9";
        dict["MobileCameraFront"] = "\uf2c9";
        dict["mobile_camera_rear"] = "\uf2c8";
        dict["MobileCameraRear"] = "\uf2c8";
        dict["mobile_cancel"] = "\uf2ea";
        dict["MobileCancel"] = "\uf2ea";
        dict["mobile_cast"] = "\uf2cc";
        dict["MobileCast"] = "\uf2cc";
        dict["mobile_charge"] = "\uf2e3";
        dict["MobileCharge"] = "\uf2e3";
        dict["mobile_chat"] = "\uf79f";
        dict["MobileChat"] = "\uf79f";
        dict["mobile_check"] = "\uf073";
        dict["MobileCheck"] = "\uf073";
        dict["mobile_code"] = "\uf2e2";
        dict["MobileCode"] = "\uf2e2";
        dict["mobile_dock"] = "\uf2e0";
        dict["MobileDock"] = "\uf2e0";
        dict["mobile_dots"] = "\uf2d0";
        dict["MobileDots"] = "\uf2d0";
        dict["mobile_friendly"] = "\uf073";
        dict["MobileFriendly"] = "\uf073";
        dict["mobile_gear"] = "\uf2d9";
        dict["MobileGear"] = "\uf2d9";
        dict["mobile_hand"] = "\uf323";
        dict["MobileHand"] = "\uf323";
        dict["mobile_hand_left"] = "\uf313";
        dict["MobileHandLeft"] = "\uf313";
        dict["mobile_hand_left_off"] = "\uf312";
        dict["MobileHandLeftOff"] = "\uf312";
        dict["mobile_hand_off"] = "\uf314";
        dict["MobileHandOff"] = "\uf314";
        dict["mobile_info"] = "\uf2dc";
        dict["MobileInfo"] = "\uf2dc";
        dict["mobile_landscape"] = "\ued3e";
        dict["MobileLandscape"] = "\ued3e";
        dict["mobile_layout"] = "\uf2bf";
        dict["MobileLayout"] = "\uf2bf";
        dict["mobile_lock_landscape"] = "\uf2d8";
        dict["MobileLockLandscape"] = "\uf2d8";
        dict["mobile_lock_portrait"] = "\uf2be";
        dict["MobileLockPortrait"] = "\uf2be";
        dict["mobile_loupe"] = "\uf322";
        dict["MobileLoupe"] = "\uf322";
        dict["mobile_menu"] = "\uf2d1";
        dict["MobileMenu"] = "\uf2d1";
        dict["mobile_off"] = "\ue201";
        dict["MobileOff"] = "\ue201";
        dict["mobile_question"] = "\uf2e1";
        dict["MobileQuestion"] = "\uf2e1";
        dict["mobile_rotate"] = "\uf2d5";
        dict["MobileRotate"] = "\uf2d5";
        dict["mobile_rotate_lock"] = "\uf2d6";
        dict["MobileRotateLock"] = "\uf2d6";
        dict["mobile_screen_share"] = "\uf2df";
        dict["MobileScreenShare"] = "\uf2df";
        dict["mobile_screensaver"] = "\uf321";
        dict["MobileScreensaver"] = "\uf321";
        dict["mobile_sensor_hi"] = "\uf2ef";
        dict["MobileSensorHi"] = "\uf2ef";
        dict["mobile_sensor_lo"] = "\uf2ee";
        dict["MobileSensorLo"] = "\uf2ee";
        dict["mobile_share"] = "\uf2df";
        dict["MobileShare"] = "\uf2df";
        dict["mobile_share_stack"] = "\uf2de";
        dict["MobileShareStack"] = "\uf2de";
        dict["mobile_sound"] = "\uf2e8";
        dict["MobileSound"] = "\uf2e8";
        dict["mobile_sound_2"] = "\uf318";
        dict["MobileSound2"] = "\uf318";
        dict["mobile_sound_off"] = "\uf7aa";
        dict["MobileSoundOff"] = "\uf7aa";
        dict["mobile_speaker"] = "\uf320";
        dict["MobileSpeaker"] = "\uf320";
        dict["mobile_tap"] = "\U000FFEB2";
        dict["MobileTap"] = "\U000FFEB2";
        dict["mobile_text"] = "\uf2eb";
        dict["MobileText"] = "\uf2eb";
        dict["mobile_text_2"] = "\uf2e6";
        dict["MobileText2"] = "\uf2e6";
        dict["mobile_theft"] = "\uf2a9";
        dict["MobileTheft"] = "\uf2a9";
        dict["mobile_ticket"] = "\uf2e4";
        dict["MobileTicket"] = "\uf2e4";
        dict["mobile_unlock"] = "\ueeea";
        dict["MobileUnlock"] = "\ueeea";
        dict["mobile_vibrate"] = "\uf2cb";
        dict["MobileVibrate"] = "\uf2cb";
        dict["mobile_wrench"] = "\uf2b0";
        dict["MobileWrench"] = "\uf2b0";
        dict["mobiledata_arrows"] = "\U000FFFA3";
        dict["MobiledataArrows"] = "\U000FFFA3";
        dict["mobiledata_off"] = "\uf034";
        dict["MobiledataOff"] = "\uf034";
        dict["mode"] = "\uf097";
        dict["mode_comment"] = "\ue253";
        dict["ModeComment"] = "\ue253";
        dict["mode_cool"] = "\uf166";
        dict["ModeCool"] = "\uf166";
        dict["mode_cool_off"] = "\uf167";
        dict["ModeCoolOff"] = "\uf167";
        dict["mode_dual"] = "\uf557";
        dict["ModeDual"] = "\uf557";
        dict["mode_edit"] = "\uf097";
        dict["ModeEdit"] = "\uf097";
        dict["mode_edit_outline"] = "\uf097";
        dict["ModeEditOutline"] = "\uf097";
        dict["mode_fan"] = "\uf168";
        dict["ModeFan"] = "\uf168";
        dict["mode_fan_2"] = "\U000FFFD0";
        dict["ModeFan2"] = "\U000FFFD0";
        dict["mode_fan_off"] = "\uec17";
        dict["ModeFanOff"] = "\uec17";
        dict["mode_heat"] = "\uf16a";
        dict["ModeHeat"] = "\uf16a";
        dict["mode_heat_cool"] = "\uf16b";
        dict["ModeHeatCool"] = "\uf16b";
        dict["mode_heat_off"] = "\uf16d";
        dict["ModeHeatOff"] = "\uf16d";
        dict["mode_night"] = "\uf036";
        dict["ModeNight"] = "\uf036";
        dict["mode_of_travel"] = "\ue7ce";
        dict["ModeOfTravel"] = "\ue7ce";
        dict["mode_off_on"] = "\uf16f";
        dict["ModeOffOn"] = "\uf16f";
        dict["mode_standby"] = "\uf037";
        dict["ModeStandby"] = "\uf037";
        dict["model_training"] = "\uf0cf";
        dict["ModelTraining"] = "\uf0cf";
        dict["modeling"] = "\uf3aa";
        dict["monetization_on"] = "\ue263";
        dict["MonetizationOn"] = "\ue263";
        dict["money"] = "\ue57d";
        dict["money_bag"] = "\uf3ee";
        dict["MoneyBag"] = "\uf3ee";
        dict["money_off"] = "\uf038";
        dict["MoneyOff"] = "\uf038";
        dict["money_off_csred"] = "\uf038";
        dict["MoneyOffCsred"] = "\uf038";
        dict["money_range"] = "\uf245";
        dict["MoneyRange"] = "\uf245";
        dict["monitor"] = "\uef5b";
        dict["monitor_heart"] = "\ueaa2";
        dict["MonitorHeart"] = "\ueaa2";
        dict["monitor_weight"] = "\uf039";
        dict["MonitorWeight"] = "\uf039";
        dict["monitor_weight_gain"] = "\uf6df";
        dict["MonitorWeightGain"] = "\uf6df";
        dict["monitor_weight_loss"] = "\uf6de";
        dict["MonitorWeightLoss"] = "\uf6de";
        dict["monitoring"] = "\uf190";
        dict["monochrome_photos"] = "\ue403";
        dict["MonochromePhotos"] = "\ue403";
        dict["monorail"] = "\uf473";
        dict["mood"] = "\uea22";
        dict["mood_bad"] = "\ue7f3";
        dict["MoodBad"] = "\ue7f3";
        dict["mood_heart"] = "\U000FFFB4";
        dict["MoodHeart"] = "\U000FFFB4";
        dict["moon_stars"] = "\uf34f";
        dict["MoonStars"] = "\uf34f";
        dict["mop"] = "\ue28d";
        dict["moped"] = "\ueb28";
        dict["moped_package"] = "\uf28b";
        dict["MopedPackage"] = "\uf28b";
        dict["more"] = "\ue619";
        dict["more_down"] = "\uf196";
        dict["MoreDown"] = "\uf196";
        dict["more_horiz"] = "\ue5d3";
        dict["MoreHoriz"] = "\ue5d3";
        dict["more_time"] = "\uea5d";
        dict["MoreTime"] = "\uea5d";
        dict["more_up"] = "\uf197";
        dict["MoreUp"] = "\uf197";
        dict["more_vert"] = "\ue5d4";
        dict["MoreVert"] = "\ue5d4";
        dict["mosque"] = "\ueab2";
        dict["motion_blur"] = "\uf0d0";
        dict["MotionBlur"] = "\uf0d0";
        dict["motion_mode"] = "\uf842";
        dict["MotionMode"] = "\uf842";
        dict["motion_photos_auto"] = "\uf03a";
        dict["MotionPhotosAuto"] = "\uf03a";
        dict["motion_photos_off"] = "\ue9c0";
        dict["MotionPhotosOff"] = "\ue9c0";
        dict["motion_photos_on"] = "\ue9c1";
        dict["MotionPhotosOn"] = "\ue9c1";
        dict["motion_photos_pause"] = "\uf227";
        dict["MotionPhotosPause"] = "\uf227";
        dict["motion_photos_paused"] = "\uf227";
        dict["MotionPhotosPaused"] = "\uf227";
        dict["motion_play"] = "\uf40b";
        dict["MotionPlay"] = "\uf40b";
        dict["motion_sensor_active"] = "\ue792";
        dict["MotionSensorActive"] = "\ue792";
        dict["motion_sensor_alert"] = "\ue784";
        dict["MotionSensorAlert"] = "\ue784";
        dict["motion_sensor_idle"] = "\ue783";
        dict["MotionSensorIdle"] = "\ue783";
        dict["motion_sensor_urgent"] = "\ue78e";
        dict["MotionSensorUrgent"] = "\ue78e";
        dict["motorcycle"] = "\ue91b";
        dict["mountain_flag"] = "\uf5e2";
        dict["MountainFlag"] = "\uf5e2";
        dict["mountain_steam"] = "\uf282";
        dict["MountainSteam"] = "\uf282";
        dict["mouse"] = "\ue323";
        dict["mouse_lock"] = "\uf490";
        dict["MouseLock"] = "\uf490";
        dict["mouse_lock_off"] = "\uf48f";
        dict["MouseLockOff"] = "\uf48f";
        dict["move"] = "\ue740";
        dict["move_down"] = "\ueb61";
        dict["MoveDown"] = "\ueb61";
        dict["move_group"] = "\uf715";
        dict["MoveGroup"] = "\uf715";
        dict["move_item"] = "\uf1ff";
        dict["MoveItem"] = "\uf1ff";
        dict["move_location"] = "\ue741";
        dict["MoveLocation"] = "\ue741";
        dict["move_selection_down"] = "\uf714";
        dict["MoveSelectionDown"] = "\uf714";
        dict["move_selection_left"] = "\uf713";
        dict["MoveSelectionLeft"] = "\uf713";
        dict["move_selection_right"] = "\uf712";
        dict["MoveSelectionRight"] = "\uf712";
        dict["move_selection_up"] = "\uf711";
        dict["MoveSelectionUp"] = "\uf711";
        dict["move_to_inbox"] = "\ue168";
        dict["MoveToInbox"] = "\ue168";
        dict["move_up"] = "\ueb64";
        dict["MoveUp"] = "\ueb64";
        dict["moved_location"] = "\ue594";
        dict["MovedLocation"] = "\ue594";
        dict["movie"] = "\ue404";
        dict["movie_creation"] = "\ue404";
        dict["MovieCreation"] = "\ue404";
        dict["movie_edit"] = "\uf840";
        dict["MovieEdit"] = "\uf840";
        dict["movie_edit_off"] = "\U000FFF7D";
        dict["MovieEditOff"] = "\U000FFF7D";
        dict["movie_filter"] = "\ue43a";
        dict["MovieFilter"] = "\ue43a";
        dict["movie_info"] = "\ue02d";
        dict["MovieInfo"] = "\ue02d";
        dict["movie_off"] = "\uf499";
        dict["MovieOff"] = "\uf499";
        dict["movie_speaker"] = "\uf2a3";
        dict["MovieSpeaker"] = "\uf2a3";
        dict["moving"] = "\ue501";
        dict["moving_beds"] = "\ue73d";
        dict["MovingBeds"] = "\ue73d";
        dict["moving_ministry"] = "\ue73e";
        dict["MovingMinistry"] = "\ue73e";
        dict["mp"] = "\ue9c3";
        dict["multicooker"] = "\ue293";
        dict["multiline_chart"] = "\ue6df";
        dict["MultilineChart"] = "\ue6df";
        dict["multimodal_hand_eye"] = "\uf41b";
        dict["MultimodalHandEye"] = "\uf41b";
        dict["multiple_airports"] = "\uefab";
        dict["MultipleAirports"] = "\uefab";
        dict["multiple_stop"] = "\uf1b9";
        dict["MultipleStop"] = "\uf1b9";
        dict["museum"] = "\uea36";
        dict["music_cast"] = "\ueb1a";
        dict["MusicCast"] = "\ueb1a";
        dict["music_history"] = "\uf2c1";
        dict["MusicHistory"] = "\uf2c1";
        dict["music_note"] = "\ue405";
        dict["MusicNote"] = "\ue405";
        dict["music_note_2"] = "\U000FFFD8";
        dict["MusicNote2"] = "\U000FFFD8";
        dict["music_note_add"] = "\uf391";
        dict["MusicNoteAdd"] = "\uf391";
        dict["music_off"] = "\ue440";
        dict["MusicOff"] = "\ue440";
        dict["music_video"] = "\ue063";
        dict["MusicVideo"] = "\ue063";
        dict["my_location"] = "\ue55c";
        dict["MyLocation"] = "\ue55c";
        dict["mystery"] = "\uf5e1";
        dict["nat"] = "\uef5c";
        dict["nature"] = "\ue406";
        dict["nature_people"] = "\ue407";
        dict["NaturePeople"] = "\ue407";
        dict["navigate_before"] = "\ue5cb";
        dict["NavigateBefore"] = "\ue5cb";
        dict["navigate_next"] = "\ue5cc";
        dict["NavigateNext"] = "\ue5cc";
        dict["navigation"] = "\ue55d";
        dict["near_me"] = "\ue569";
        dict["NearMe"] = "\ue569";
        dict["near_me_disabled"] = "\uf1ef";
        dict["NearMeDisabled"] = "\uf1ef";
        dict["nearby"] = "\ue6b7";
        dict["nearby_error"] = "\uf03b";
        dict["NearbyError"] = "\uf03b";
        dict["nearby_off"] = "\uf03c";
        dict["NearbyOff"] = "\uf03c";
        dict["nephrology"] = "\ue10d";
        dict["nest_audio"] = "\uebbf";
        dict["NestAudio"] = "\uebbf";
        dict["nest_cam_floodlight"] = "\uf8b7";
        dict["NestCamFloodlight"] = "\uf8b7";
        dict["nest_cam_indoor"] = "\uf11e";
        dict["NestCamIndoor"] = "\uf11e";
        dict["nest_cam_iq"] = "\uf11f";
        dict["NestCamIq"] = "\uf11f";
        dict["nest_cam_iq_outdoor"] = "\uf120";
        dict["NestCamIqOutdoor"] = "\uf120";
        dict["nest_cam_magnet_mount"] = "\uf8b8";
        dict["NestCamMagnetMount"] = "\uf8b8";
        dict["nest_cam_outdoor"] = "\uf121";
        dict["NestCamOutdoor"] = "\uf121";
        dict["nest_cam_stand"] = "\uf8b9";
        dict["NestCamStand"] = "\uf8b9";
        dict["nest_cam_wall_mount"] = "\uf8ba";
        dict["NestCamWallMount"] = "\uf8ba";
        dict["nest_cam_wired_stand"] = "\uec16";
        dict["NestCamWiredStand"] = "\uec16";
        dict["nest_clock_farsight_analog"] = "\uf8bb";
        dict["NestClockFarsightAnalog"] = "\uf8bb";
        dict["nest_clock_farsight_digital"] = "\uf8bc";
        dict["NestClockFarsightDigital"] = "\uf8bc";
        dict["nest_connect"] = "\uf122";
        dict["NestConnect"] = "\uf122";
        dict["nest_detect"] = "\uf123";
        dict["NestDetect"] = "\uf123";
        dict["nest_display"] = "\uf124";
        dict["NestDisplay"] = "\uf124";
        dict["nest_display_max"] = "\uf125";
        dict["NestDisplayMax"] = "\uf125";
        dict["nest_doorbell_visitor"] = "\uf8bd";
        dict["NestDoorbellVisitor"] = "\uf8bd";
        dict["nest_eco_leaf"] = "\uf8be";
        dict["NestEcoLeaf"] = "\uf8be";
        dict["nest_farsight_cool"] = "\uf27d";
        dict["NestFarsightCool"] = "\uf27d";
        dict["nest_farsight_dual"] = "\uf27c";
        dict["NestFarsightDual"] = "\uf27c";
        dict["nest_farsight_eco"] = "\uf27b";
        dict["NestFarsightEco"] = "\uf27b";
        dict["nest_farsight_heat"] = "\uf27a";
        dict["NestFarsightHeat"] = "\uf27a";
        dict["nest_farsight_seasonal"] = "\uf279";
        dict["NestFarsightSeasonal"] = "\uf279";
        dict["nest_farsight_weather"] = "\uf8bf";
        dict["NestFarsightWeather"] = "\uf8bf";
        dict["nest_found_savings"] = "\uf8c0";
        dict["NestFoundSavings"] = "\uf8c0";
        dict["nest_gale_wifi"] = "\uf579";
        dict["NestGaleWifi"] = "\uf579";
        dict["nest_heat_link_e"] = "\uf126";
        dict["NestHeatLinkE"] = "\uf126";
        dict["nest_heat_link_gen_3"] = "\uf127";
        dict["NestHeatLinkGen3"] = "\uf127";
        dict["nest_hello_doorbell"] = "\ue82c";
        dict["NestHelloDoorbell"] = "\ue82c";
        dict["nest_locator_tag"] = "\uf8c1";
        dict["NestLocatorTag"] = "\uf8c1";
        dict["nest_mini"] = "\ue789";
        dict["NestMini"] = "\ue789";
        dict["nest_multi_room"] = "\uf8c2";
        dict["NestMultiRoom"] = "\uf8c2";
        dict["nest_protect"] = "\ue68e";
        dict["NestProtect"] = "\ue68e";
        dict["nest_remote"] = "\uf5db";
        dict["NestRemote"] = "\uf5db";
        dict["nest_remote_comfort_sensor"] = "\uf12a";
        dict["NestRemoteComfortSensor"] = "\uf12a";
        dict["nest_secure_alarm"] = "\uf12b";
        dict["NestSecureAlarm"] = "\uf12b";
        dict["nest_sunblock"] = "\uf8c3";
        dict["NestSunblock"] = "\uf8c3";
        dict["nest_tag"] = "\uf8c1";
        dict["NestTag"] = "\uf8c1";
        dict["nest_thermostat"] = "\ue68f";
        dict["NestThermostat"] = "\ue68f";
        dict["nest_thermostat_e_eu"] = "\uf12d";
        dict["NestThermostatEEu"] = "\uf12d";
        dict["nest_thermostat_gen_3"] = "\uf12e";
        dict["NestThermostatGen3"] = "\uf12e";
        dict["nest_thermostat_sensor"] = "\uf12f";
        dict["NestThermostatSensor"] = "\uf12f";
        dict["nest_thermostat_sensor_eu"] = "\uf130";
        dict["NestThermostatSensorEu"] = "\uf130";
        dict["nest_thermostat_zirconium_eu"] = "\uf131";
        dict["NestThermostatZirconiumEu"] = "\uf131";
        dict["nest_true_radiant"] = "\uf8c4";
        dict["NestTrueRadiant"] = "\uf8c4";
        dict["nest_wake_on_approach"] = "\uf8c5";
        dict["NestWakeOnApproach"] = "\uf8c5";
        dict["nest_wake_on_press"] = "\uf8c6";
        dict["NestWakeOnPress"] = "\uf8c6";
        dict["nest_wifi_gale"] = "\uf132";
        dict["NestWifiGale"] = "\uf132";
        dict["nest_wifi_mistral"] = "\uf133";
        dict["NestWifiMistral"] = "\uf133";
        dict["nest_wifi_point"] = "\uf134";
        dict["NestWifiPoint"] = "\uf134";
        dict["nest_wifi_point_vento"] = "\uf134";
        dict["NestWifiPointVento"] = "\uf134";
        dict["nest_wifi_pro"] = "\uf56b";
        dict["NestWifiPro"] = "\uf56b";
        dict["nest_wifi_pro_2"] = "\uf56a";
        dict["NestWifiPro2"] = "\uf56a";
        dict["nest_wifi_router"] = "\uf133";
        dict["NestWifiRouter"] = "\uf133";
        dict["network_cell"] = "\ue1b9";
        dict["NetworkCell"] = "\ue1b9";
        dict["network_check"] = "\ue640";
        dict["NetworkCheck"] = "\ue640";
        dict["network_intel_node"] = "\uf371";
        dict["NetworkIntelNode"] = "\uf371";
        dict["network_intelligence"] = "\uefac";
        dict["NetworkIntelligence"] = "\uefac";
        dict["network_intelligence_history"] = "\uf5f6";
        dict["NetworkIntelligenceHistory"] = "\uf5f6";
        dict["network_intelligence_update"] = "\uf5f5";
        dict["NetworkIntelligenceUpdate"] = "\uf5f5";
        dict["network_locked"] = "\ue61a";
        dict["NetworkLocked"] = "\ue61a";
        dict["network_manage"] = "\uf7ab";
        dict["NetworkManage"] = "\uf7ab";
        dict["network_node"] = "\uf56e";
        dict["NetworkNode"] = "\uf56e";
        dict["network_ping"] = "\uebca";
        dict["NetworkPing"] = "\uebca";
        dict["network_wifi"] = "\ue1ba";
        dict["NetworkWifi"] = "\ue1ba";
        dict["network_wifi_1_bar"] = "\uebe4";
        dict["NetworkWifi1Bar"] = "\uebe4";
        dict["network_wifi_1_bar_locked"] = "\uf58f";
        dict["NetworkWifi1BarLocked"] = "\uf58f";
        dict["network_wifi_2_bar"] = "\uebd6";
        dict["NetworkWifi2Bar"] = "\uebd6";
        dict["network_wifi_2_bar_locked"] = "\uf58e";
        dict["NetworkWifi2BarLocked"] = "\uf58e";
        dict["network_wifi_3_bar"] = "\uebe1";
        dict["NetworkWifi3Bar"] = "\uebe1";
        dict["network_wifi_3_bar_locked"] = "\uf58d";
        dict["NetworkWifi3BarLocked"] = "\uf58d";
        dict["network_wifi_locked"] = "\uf532";
        dict["NetworkWifiLocked"] = "\uf532";
        dict["neurology"] = "\ue10e";
        dict["new_label"] = "\ue609";
        dict["NewLabel"] = "\ue609";
        dict["new_releases"] = "\uef76";
        dict["NewReleases"] = "\uef76";
        dict["new_window"] = "\uf710";
        dict["NewWindow"] = "\uf710";
        dict["news"] = "\ue032";
        dict["newsmode"] = "\uefad";
        dict["newspaper"] = "\ueb81";
        dict["newsstand"] = "\ue9c4";
        dict["next_plan"] = "\uef5d";
        dict["NextPlan"] = "\uef5d";
        dict["next_week"] = "\ue16a";
        dict["NextWeek"] = "\ue16a";
        dict["nfc"] = "\ue1bb";
        dict["nfc_off"] = "\uf369";
        dict["NfcOff"] = "\uf369";
        dict["night_shelter"] = "\uf1f1";
        dict["NightShelter"] = "\uf1f1";
        dict["night_sight_auto"] = "\uf1d7";
        dict["NightSightAuto"] = "\uf1d7";
        dict["night_sight_auto_off"] = "\uf1f9";
        dict["NightSightAutoOff"] = "\uf1f9";
        dict["night_sight_max"] = "\uf6c3";
        dict["NightSightMax"] = "\uf6c3";
        dict["nightlife"] = "\uea62";
        dict["nightlight"] = "\uf03d";
        dict["nightlight_round"] = "\uf03d";
        dict["NightlightRound"] = "\uf03d";
        dict["nights_stay"] = "\uf174";
        dict["NightsStay"] = "\uf174";
        dict["no_accounts"] = "\uf03e";
        dict["NoAccounts"] = "\uf03e";
        dict["no_adult_content"] = "\uf8fe";
        dict["NoAdultContent"] = "\uf8fe";
        dict["no_backpack"] = "\uf237";
        dict["NoBackpack"] = "\uf237";
        dict["no_crash"] = "\uebf0";
        dict["NoCrash"] = "\uebf0";
        dict["no_drinks"] = "\uf1a5";
        dict["NoDrinks"] = "\uf1a5";
        dict["no_encryption"] = "\uf03f";
        dict["NoEncryption"] = "\uf03f";
        dict["no_encryption_gmailerrorred"] = "\uf03f";
        dict["NoEncryptionGmailerrorred"] = "\uf03f";
        dict["no_flash"] = "\uf1a6";
        dict["NoFlash"] = "\uf1a6";
        dict["no_food"] = "\uf1a7";
        dict["NoFood"] = "\uf1a7";
        dict["no_luggage"] = "\uf23b";
        dict["NoLuggage"] = "\uf23b";
        dict["no_meals"] = "\uf1d6";
        dict["NoMeals"] = "\uf1d6";
        dict["no_meeting_room"] = "\ueb4e";
        dict["NoMeetingRoom"] = "\ueb4e";
        dict["no_photography"] = "\uf1a8";
        dict["NoPhotography"] = "\uf1a8";
        dict["no_sim"] = "\ue1ce";
        dict["NoSim"] = "\ue1ce";
        dict["no_sound"] = "\ue710";
        dict["NoSound"] = "\ue710";
        dict["no_stroller"] = "\uf1af";
        dict["NoStroller"] = "\uf1af";
        dict["no_transfer"] = "\uf1d5";
        dict["NoTransfer"] = "\uf1d5";
        dict["noise_aware"] = "\uebec";
        dict["NoiseAware"] = "\uebec";
        dict["noise_control_off"] = "\uebf3";
        dict["NoiseControlOff"] = "\uebf3";
        dict["noise_control_on"] = "\uf8a8";
        dict["NoiseControlOn"] = "\uf8a8";
        dict["nordic_walking"] = "\ue50e";
        dict["NordicWalking"] = "\ue50e";
        dict["north"] = "\uf1e0";
        dict["north_east"] = "\uf1e1";
        dict["NorthEast"] = "\uf1e1";
        dict["north_west"] = "\uf1e2";
        dict["NorthWest"] = "\uf1e2";
        dict["not_accessible"] = "\uf0fe";
        dict["NotAccessible"] = "\uf0fe";
        dict["not_accessible_forward"] = "\uf54a";
        dict["NotAccessibleForward"] = "\uf54a";
        dict["not_interested"] = "\uf08c";
        dict["NotInterested"] = "\uf08c";
        dict["not_listed_location"] = "\ue575";
        dict["NotListedLocation"] = "\ue575";
        dict["not_started"] = "\uf0d1";
        dict["NotStarted"] = "\uf0d1";
        dict["note"] = "\ue66d";
        dict["note_add"] = "\ue89c";
        dict["NoteAdd"] = "\ue89c";
        dict["note_alt"] = "\uf040";
        dict["NoteAlt"] = "\uf040";
        dict["note_stack"] = "\uf562";
        dict["NoteStack"] = "\uf562";
        dict["note_stack_add"] = "\uf563";
        dict["NoteStackAdd"] = "\uf563";
        dict["notes"] = "\ue26c";
        dict["notification_add"] = "\ue399";
        dict["NotificationAdd"] = "\ue399";
        dict["notification_audio"] = "\ueec1";
        dict["NotificationAudio"] = "\ueec1";
        dict["notification_audio_off"] = "\ueec0";
        dict["NotificationAudioOff"] = "\ueec0";
        dict["notification_important"] = "\ue004";
        dict["NotificationImportant"] = "\ue004";
        dict["notification_multiple"] = "\ue6c2";
        dict["NotificationMultiple"] = "\ue6c2";
        dict["notification_settings"] = "\uf367";
        dict["NotificationSettings"] = "\uf367";
        dict["notification_sound"] = "\uf353";
        dict["NotificationSound"] = "\uf353";
        dict["notifications"] = "\ue7f5";
        dict["notifications_active"] = "\ue7f7";
        dict["NotificationsActive"] = "\ue7f7";
        dict["notifications_none"] = "\ue7f5";
        dict["NotificationsNone"] = "\ue7f5";
        dict["notifications_off"] = "\ue7f6";
        dict["NotificationsOff"] = "\ue7f6";
        dict["notifications_paused"] = "\ue7f8";
        dict["NotificationsPaused"] = "\ue7f8";
        dict["notifications_unread"] = "\uf4fe";
        dict["NotificationsUnread"] = "\uf4fe";
        dict["numbers"] = "\ueac7";
        dict["nutrition"] = "\ue110";
        dict["ods"] = "\ue6e8";
        dict["odt"] = "\ue6e9";
        dict["offline_bolt"] = "\ue932";
        dict["OfflineBolt"] = "\ue932";
        dict["offline_pin"] = "\ue90a";
        dict["OfflinePin"] = "\ue90a";
        dict["offline_pin_off"] = "\uf4d0";
        dict["OfflinePinOff"] = "\uf4d0";
        dict["offline_share"] = "\uf2de";
        dict["OfflineShare"] = "\uf2de";
        dict["oil_barrel"] = "\uec15";
        dict["OilBarrel"] = "\uec15";
        dict["okonomiyaki"] = "\uf281";
        dict["on_device_training"] = "\uebfd";
        dict["OnDeviceTraining"] = "\uebfd";
        dict["on_hub_device"] = "\ue6c3";
        dict["OnHubDevice"] = "\ue6c3";
        dict["oncology"] = "\ue114";
        dict["ondemand_video"] = "\ue63a";
        dict["OndemandVideo"] = "\ue63a";
        dict["online_prediction"] = "\uf0eb";
        dict["OnlinePrediction"] = "\uf0eb";
        dict["onsen"] = "\uf6f8";
        dict["opacity"] = "\ue91c";
        dict["open_in_browser"] = "\ue89d";
        dict["OpenInBrowser"] = "\ue89d";
        dict["open_in_full"] = "\uf1ce";
        dict["OpenInFull"] = "\uf1ce";
        dict["open_in_new"] = "\ue89e";
        dict["OpenInNew"] = "\ue89e";
        dict["open_in_new_down"] = "\uf70f";
        dict["OpenInNewDown"] = "\uf70f";
        dict["open_in_new_off"] = "\ue4f6";
        dict["OpenInNewOff"] = "\ue4f6";
        dict["open_in_phone"] = "\uf2d2";
        dict["OpenInPhone"] = "\uf2d2";
        dict["open_jam"] = "\uefae";
        dict["OpenJam"] = "\uefae";
        dict["open_run"] = "\uf4b7";
        dict["OpenRun"] = "\uf4b7";
        dict["open_with"] = "\ue89f";
        dict["OpenWith"] = "\ue89f";
        dict["ophthalmology"] = "\ue115";
        dict["oral_disease"] = "\ue116";
        dict["OralDisease"] = "\ue116";
        dict["orbit"] = "\uf426";
        dict["order_approve"] = "\uf812";
        dict["OrderApprove"] = "\uf812";
        dict["order_play"] = "\uf811";
        dict["OrderPlay"] = "\uf811";
        dict["orders"] = "\ueb14";
        dict["orthopedics"] = "\uf897";
        dict["other_admission"] = "\ue47b";
        dict["OtherAdmission"] = "\ue47b";
        dict["other_houses"] = "\ue58c";
        dict["OtherHouses"] = "\ue58c";
        dict["outbound"] = "\ue1ca";
        dict["outbox"] = "\uef5f";
        dict["outbox_alt"] = "\ueb17";
        dict["OutboxAlt"] = "\ueb17";
        dict["outdoor_garden"] = "\ue205";
        dict["OutdoorGarden"] = "\ue205";
        dict["outdoor_grill"] = "\uea47";
        dict["OutdoorGrill"] = "\uea47";
        dict["outgoing_mail"] = "\uf0d2";
        dict["OutgoingMail"] = "\uf0d2";
        dict["outlet"] = "\uf1d4";
        dict["outlined_flag"] = "\uf0c6";
        dict["OutlinedFlag"] = "\uf0c6";
        dict["outpatient"] = "\ue118";
        dict["outpatient_med"] = "\ue119";
        dict["OutpatientMed"] = "\ue119";
        dict["output"] = "\uebbe";
        dict["output_circle"] = "\uf70e";
        dict["OutputCircle"] = "\uf70e";
        dict["oven"] = "\ue9c7";
        dict["oven_gen"] = "\ue843";
        dict["OvenGen"] = "\ue843";
        dict["overview"] = "\ue4a7";
        dict["overview_key"] = "\uf7d4";
        dict["OverviewKey"] = "\uf7d4";
        dict["owl"] = "\uf3b4";
        dict["oxygen_saturation"] = "\ue4de";
        dict["OxygenSaturation"] = "\ue4de";
        dict["p2p"] = "\uf52a";
        dict["pace"] = "\uf6b8";
        dict["pacemaker"] = "\ue656";
        dict["package"] = "\ue48f";
        dict["package_2"] = "\uf569";
        dict["Package2"] = "\uf569";
        dict["padding"] = "\ue9c8";
        dict["padel"] = "\uf2a7";
        dict["page_control"] = "\ue731";
        dict["PageControl"] = "\ue731";
        dict["page_footer"] = "\uf383";
        dict["PageFooter"] = "\uf383";
        dict["page_header"] = "\uf384";
        dict["PageHeader"] = "\uf384";
        dict["page_info"] = "\uf614";
        dict["PageInfo"] = "\uf614";
        dict["page_menu_ios"] = "\ueefb";
        dict["PageMenuIos"] = "\ueefb";
        dict["pageless"] = "\uf509";
        dict["pages"] = "\ue7f9";
        dict["pageview"] = "\ue8a0";
        dict["paid"] = "\uf041";
        dict["palette"] = "\ue40a";
        dict["pallet"] = "\uf86a";
        dict["pan_tool"] = "\ue925";
        dict["PanTool"] = "\ue925";
        dict["pan_tool_alt"] = "\uebb9";
        dict["PanToolAlt"] = "\uebb9";
        dict["pan_zoom"] = "\uf655";
        dict["PanZoom"] = "\uf655";
        dict["panorama"] = "\ue691";
        dict["panorama_fish_eye"] = "\ue40c";
        dict["PanoramaFishEye"] = "\ue40c";
        dict["panorama_horizontal"] = "\ue40d";
        dict["PanoramaHorizontal"] = "\ue40d";
        dict["panorama_photosphere"] = "\ue9c9";
        dict["PanoramaPhotosphere"] = "\ue9c9";
        dict["panorama_vertical"] = "\ue40e";
        dict["PanoramaVertical"] = "\ue40e";
        dict["panorama_wide_angle"] = "\ue40f";
        dict["PanoramaWideAngle"] = "\ue40f";
        dict["paragliding"] = "\ue50f";
        dict["parent_child_dining"] = "\uf22d";
        dict["ParentChildDining"] = "\uf22d";
        dict["park"] = "\uea63";
        dict["parking_meter"] = "\uf28a";
        dict["ParkingMeter"] = "\uf28a";
        dict["parking_sign"] = "\uf289";
        dict["ParkingSign"] = "\uf289";
        dict["parking_valet"] = "\uf288";
        dict["ParkingValet"] = "\uf288";
        dict["partly_cloudy_day"] = "\uf172";
        dict["PartlyCloudyDay"] = "\uf172";
        dict["partly_cloudy_night"] = "\uf174";
        dict["PartlyCloudyNight"] = "\uf174";
        dict["partner_exchange"] = "\uf7f9";
        dict["PartnerExchange"] = "\uf7f9";
        dict["partner_heart"] = "\uef2e";
        dict["PartnerHeart"] = "\uef2e";
        dict["partner_reports"] = "\uefaf";
        dict["PartnerReports"] = "\uefaf";
        dict["party_mode"] = "\ue7fa";
        dict["PartyMode"] = "\ue7fa";
        dict["passkey"] = "\uf87f";
        dict["passport"] = "\ueec4";
        dict["password"] = "\uf042";
        dict["password_2"] = "\uf4a9";
        dict["Password2"] = "\uf4a9";
        dict["password_2_off"] = "\uf4a8";
        dict["Password2Off"] = "\uf4a8";
        dict["patient_list"] = "\ue653";
        dict["PatientList"] = "\ue653";
        dict["pattern"] = "\uf043";
        dict["pause"] = "\ue034";
        dict["pause_circle"] = "\ue1a2";
        dict["PauseCircle"] = "\ue1a2";
        dict["pause_circle_filled"] = "\ue1a2";
        dict["PauseCircleFilled"] = "\ue1a2";
        dict["pause_circle_outline"] = "\ue1a2";
        dict["PauseCircleOutline"] = "\ue1a2";
        dict["pause_presentation"] = "\ue0ea";
        dict["PausePresentation"] = "\ue0ea";
        dict["payment"] = "\ue8a1";
        dict["payment_arrow_down"] = "\uf2c0";
        dict["PaymentArrowDown"] = "\uf2c0";
        dict["payment_card"] = "\uf2a1";
        dict["PaymentCard"] = "\uf2a1";
        dict["payments"] = "\uef63";
        dict["pedal_bike"] = "\ueb29";
        dict["PedalBike"] = "\ueb29";
        dict["pediatrics"] = "\ue11d";
        dict["pen_size_1"] = "\uf755";
        dict["PenSize1"] = "\uf755";
        dict["pen_size_2"] = "\uf754";
        dict["PenSize2"] = "\uf754";
        dict["pen_size_3"] = "\uf753";
        dict["PenSize3"] = "\uf753";
        dict["pen_size_4"] = "\uf752";
        dict["PenSize4"] = "\uf752";
        dict["pen_size_5"] = "\uf751";
        dict["PenSize5"] = "\uf751";
        dict["pending"] = "\uef64";
        dict["pending_actions"] = "\uf1bb";
        dict["PendingActions"] = "\uf1bb";
        dict["pentagon"] = "\ueb50";
        dict["people"] = "\uea21";
        dict["people_alt"] = "\uea21";
        dict["PeopleAlt"] = "\uea21";
        dict["people_outline"] = "\uea21";
        dict["PeopleOutline"] = "\uea21";
        dict["people_size_decrease"] = "\U000FFEB1";
        dict["PeopleSizeDecrease"] = "\U000FFEB1";
        dict["people_size_increase"] = "\U000FFEB0";
        dict["PeopleSizeIncrease"] = "\U000FFEB0";
        dict["percent"] = "\ueb58";
        dict["percent_discount"] = "\uf244";
        dict["PercentDiscount"] = "\uf244";
        dict["performance_max"] = "\ue51a";
        dict["PerformanceMax"] = "\ue51a";
        dict["pergola"] = "\ue203";
        dict["perm_camera_mic"] = "\ue8a2";
        dict["PermCameraMic"] = "\ue8a2";
        dict["perm_contact_calendar"] = "\ue8a3";
        dict["PermContactCalendar"] = "\ue8a3";
        dict["perm_data_setting"] = "\ue8a4";
        dict["PermDataSetting"] = "\ue8a4";
        dict["perm_device_information"] = "\uf2dc";
        dict["PermDeviceInformation"] = "\uf2dc";
        dict["perm_identity"] = "\uf0d3";
        dict["PermIdentity"] = "\uf0d3";
        dict["perm_media"] = "\ue8a7";
        dict["PermMedia"] = "\ue8a7";
        dict["perm_phone_msg"] = "\ue8a8";
        dict["PermPhoneMsg"] = "\ue8a8";
        dict["perm_scan_wifi"] = "\ue8a9";
        dict["PermScanWifi"] = "\ue8a9";
        dict["person"] = "\uf0d3";
        dict["person_2"] = "\uf8e4";
        dict["Person2"] = "\uf8e4";
        dict["person_3"] = "\uf8e5";
        dict["Person3"] = "\uf8e5";
        dict["person_4"] = "\uf8e6";
        dict["Person4"] = "\uf8e6";
        dict["person_add"] = "\uea4d";
        dict["PersonAdd"] = "\uea4d";
        dict["person_add_alt"] = "\uea4d";
        dict["PersonAddAlt"] = "\uea4d";
        dict["person_add_disabled"] = "\ue9cb";
        dict["PersonAddDisabled"] = "\ue9cb";
        dict["person_alert"] = "\uf567";
        dict["PersonAlert"] = "\uf567";
        dict["person_apron"] = "\uf5a3";
        dict["PersonApron"] = "\uf5a3";
        dict["person_book"] = "\uf5e8";
        dict["PersonBook"] = "\uf5e8";
        dict["person_cancel"] = "\uf566";
        dict["PersonCancel"] = "\uf566";
        dict["person_celebrate"] = "\uf7fe";
        dict["PersonCelebrate"] = "\uf7fe";
        dict["person_check"] = "\uf565";
        dict["PersonCheck"] = "\uf565";
        dict["person_edit"] = "\uf4fa";
        dict["PersonEdit"] = "\uf4fa";
        dict["person_filled"] = "\uf0d3";
        dict["PersonFilled"] = "\uf0d3";
        dict["person_heart"] = "\uf290";
        dict["PersonHeart"] = "\uf290";
        dict["person_off"] = "\ue510";
        dict["PersonOff"] = "\ue510";
        dict["person_outline"] = "\uf0d3";
        dict["PersonOutline"] = "\uf0d3";
        dict["person_pin"] = "\ue55a";
        dict["PersonPin"] = "\ue55a";
        dict["person_pin_circle"] = "\ue56a";
        dict["PersonPinCircle"] = "\ue56a";
        dict["person_play"] = "\uf7fd";
        dict["PersonPlay"] = "\uf7fd";
        dict["person_raised_hand"] = "\uf59a";
        dict["PersonRaisedHand"] = "\uf59a";
        dict["person_remove"] = "\uef66";
        dict["PersonRemove"] = "\uef66";
        dict["person_search"] = "\uf106";
        dict["PersonSearch"] = "\uf106";
        dict["person_shield"] = "\ue384";
        dict["PersonShield"] = "\ue384";
        dict["person_text"] = "\ueebd";
        dict["PersonText"] = "\ueebd";
        dict["personal_bag"] = "\ueb0e";
        dict["PersonalBag"] = "\ueb0e";
        dict["personal_bag_off"] = "\ueb0f";
        dict["PersonalBagOff"] = "\ueb0f";
        dict["personal_bag_question"] = "\ueb10";
        dict["PersonalBagQuestion"] = "\ueb10";
        dict["personal_injury"] = "\ue6da";
        dict["PersonalInjury"] = "\ue6da";
        dict["personal_places"] = "\ue703";
        dict["PersonalPlaces"] = "\ue703";
        dict["personal_video"] = "\ue63b";
        dict["PersonalVideo"] = "\ue63b";
        dict["pest_control"] = "\uf0fa";
        dict["PestControl"] = "\uf0fa";
        dict["pest_control_rodent"] = "\uf0fd";
        dict["PestControlRodent"] = "\uf0fd";
        dict["pet_supplies"] = "\uefb1";
        dict["PetSupplies"] = "\uefb1";
        dict["pets"] = "\ue91d";
        dict["phishing"] = "\uead7";
        dict["phone"] = "\uf0d4";
        dict["phone_alt"] = "\uf0d4";
        dict["PhoneAlt"] = "\uf0d4";
        dict["phone_android"] = "\uf2db";
        dict["PhoneAndroid"] = "\uf2db";
        dict["phone_bluetooth_speaker"] = "\ue61b";
        dict["PhoneBluetoothSpeaker"] = "\ue61b";
        dict["phone_callback"] = "\ue649";
        dict["PhoneCallback"] = "\ue649";
        dict["phone_cancel"] = "\U000FFF9D";
        dict["PhoneCancel"] = "\U000FFF9D";
        dict["phone_disabled"] = "\ue9cc";
        dict["PhoneDisabled"] = "\ue9cc";
        dict["phone_enabled"] = "\ue9cd";
        dict["PhoneEnabled"] = "\ue9cd";
        dict["phone_forwarded"] = "\ue61c";
        dict["PhoneForwarded"] = "\ue61c";
        dict["phone_in_talk"] = "\ue61d";
        dict["PhoneInTalk"] = "\ue61d";
        dict["phone_iphone"] = "\uf2da";
        dict["PhoneIphone"] = "\uf2da";
        dict["phone_locked"] = "\ue61e";
        dict["PhoneLocked"] = "\ue61e";
        dict["phone_missed"] = "\ue61f";
        dict["PhoneMissed"] = "\ue61f";
        dict["phone_paused"] = "\ue620";
        dict["PhonePaused"] = "\ue620";
        dict["phonelink"] = "\ue326";
        dict["phonelink_erase"] = "\uf2ea";
        dict["PhonelinkErase"] = "\uf2ea";
        dict["phonelink_lock"] = "\uf2be";
        dict["PhonelinkLock"] = "\uf2be";
        dict["phonelink_off"] = "\uf7a5";
        dict["PhonelinkOff"] = "\uf7a5";
        dict["phonelink_ring"] = "\uf2e8";
        dict["PhonelinkRing"] = "\uf2e8";
        dict["phonelink_ring_off"] = "\uf7aa";
        dict["PhonelinkRingOff"] = "\uf7aa";
        dict["phonelink_setup"] = "\uf2d9";
        dict["PhonelinkSetup"] = "\uf2d9";
        dict["photo"] = "\ue693";
        dict["photo_album"] = "\ue411";
        dict["PhotoAlbum"] = "\ue411";
        dict["photo_auto_merge"] = "\uf530";
        dict["PhotoAutoMerge"] = "\uf530";
        dict["photo_camera"] = "\ue412";
        dict["PhotoCamera"] = "\ue412";
        dict["photo_camera_back"] = "\uef68";
        dict["PhotoCameraBack"] = "\uef68";
        dict["photo_camera_front"] = "\uef69";
        dict["PhotoCameraFront"] = "\uef69";
        dict["photo_filter"] = "\ue43b";
        dict["PhotoFilter"] = "\ue43b";
        dict["photo_frame"] = "\uf0d9";
        dict["PhotoFrame"] = "\uf0d9";
        dict["photo_library"] = "\ue413";
        dict["PhotoLibrary"] = "\ue413";
        dict["photo_prints"] = "\uefb2";
        dict["PhotoPrints"] = "\uefb2";
        dict["photo_size_select_actual"] = "\ue693";
        dict["PhotoSizeSelectActual"] = "\ue693";
        dict["photo_size_select_large"] = "\ue433";
        dict["PhotoSizeSelectLarge"] = "\ue433";
        dict["photo_size_select_small"] = "\ue434";
        dict["PhotoSizeSelectSmall"] = "\ue434";
        dict["php"] = "\ueb8f";
        dict["physical_therapy"] = "\ue11e";
        dict["PhysicalTherapy"] = "\ue11e";
        dict["piano"] = "\ue521";
        dict["piano_off"] = "\ue520";
        dict["PianoOff"] = "\ue520";
        dict["pickleball"] = "\uf2a6";
        dict["picture_as_pdf"] = "\ue415";
        dict["PictureAsPdf"] = "\ue415";
        dict["picture_in_picture"] = "\ue8aa";
        dict["PictureInPicture"] = "\ue8aa";
        dict["picture_in_picture_alt"] = "\ue911";
        dict["PictureInPictureAlt"] = "\ue911";
        dict["picture_in_picture_center"] = "\uf550";
        dict["PictureInPictureCenter"] = "\uf550";
        dict["picture_in_picture_large"] = "\uf54f";
        dict["PictureInPictureLarge"] = "\uf54f";
        dict["picture_in_picture_medium"] = "\uf54e";
        dict["PictureInPictureMedium"] = "\uf54e";
        dict["picture_in_picture_mobile"] = "\uf517";
        dict["PictureInPictureMobile"] = "\uf517";
        dict["picture_in_picture_off"] = "\uf52f";
        dict["PictureInPictureOff"] = "\uf52f";
        dict["picture_in_picture_small"] = "\uf54d";
        dict["PictureInPictureSmall"] = "\uf54d";
        dict["pie_chart"] = "\uf0da";
        dict["PieChart"] = "\uf0da";
        dict["pie_chart_filled"] = "\uf0da";
        dict["PieChartFilled"] = "\uf0da";
        dict["pie_chart_outline"] = "\uf0da";
        dict["PieChartOutline"] = "\uf0da";
        dict["pie_chart_outlined"] = "\uf0da";
        dict["PieChartOutlined"] = "\uf0da";
        dict["pill"] = "\ue11f";
        dict["pill_off"] = "\uf809";
        dict["PillOff"] = "\uf809";
        dict["pin"] = "\uf045";
        dict["pin_drop"] = "\ue55e";
        dict["PinDrop"] = "\ue55e";
        dict["pin_end"] = "\ue767";
        dict["PinEnd"] = "\ue767";
        dict["pin_history"] = "\U000FFF2E";
        dict["PinHistory"] = "\U000FFF2E";
        dict["pin_invoke"] = "\ue763";
        dict["PinInvoke"] = "\ue763";
        dict["pin_road"] = "\U000FFF2D";
        dict["PinRoad"] = "\U000FFF2D";
        dict["pin_road_2"] = "\U000FFEEB";
        dict["PinRoad2"] = "\U000FFEEB";
        dict["pinboard"] = "\uf3ab";
        dict["pinboard_unread"] = "\uf3ac";
        dict["PinboardUnread"] = "\uf3ac";
        dict["pinch"] = "\ueb38";
        dict["pinch_zoom_in"] = "\uf1fa";
        dict["PinchZoomIn"] = "\uf1fa";
        dict["pinch_zoom_out"] = "\uf1fb";
        dict["PinchZoomOut"] = "\uf1fb";
        dict["pip"] = "\uf64d";
        dict["pip_exit"] = "\uf70d";
        dict["PipExit"] = "\uf70d";
        dict["pivot_table_chart"] = "\ue9ce";
        dict["PivotTableChart"] = "\ue9ce";
        dict["place"] = "\uf1db";
        dict["place_item"] = "\uf1f0";
        dict["PlaceItem"] = "\uf1f0";
        dict["plagiarism"] = "\uea5a";
        dict["plane_contrails"] = "\uf2ac";
        dict["PlaneContrails"] = "\uf2ac";
        dict["planet"] = "\uf387";
        dict["planner_banner_ad_pt"] = "\ue692";
        dict["PlannerBannerAdPt"] = "\ue692";
        dict["planner_review"] = "\ue694";
        dict["PlannerReview"] = "\ue694";
        dict["play_arrow"] = "\ue037";
        dict["PlayArrow"] = "\ue037";
        dict["play_circle"] = "\ue1c4";
        dict["PlayCircle"] = "\ue1c4";
        dict["play_disabled"] = "\uef6a";
        dict["PlayDisabled"] = "\uef6a";
        dict["play_for_work"] = "\ue906";
        dict["PlayForWork"] = "\ue906";
        dict["play_lesson"] = "\uf047";
        dict["PlayLesson"] = "\uf047";
        dict["play_music"] = "\ue6ee";
        dict["PlayMusic"] = "\ue6ee";
        dict["play_pause"] = "\uf137";
        dict["PlayPause"] = "\uf137";
        dict["play_shapes"] = "\uf7fc";
        dict["PlayShapes"] = "\uf7fc";
        dict["playground"] = "\uf28e";
        dict["playground_2"] = "\uf28f";
        dict["Playground2"] = "\uf28f";
        dict["playing_cards"] = "\uf5dc";
        dict["PlayingCards"] = "\uf5dc";
        dict["playlist_add"] = "\ue03b";
        dict["PlaylistAdd"] = "\ue03b";
        dict["playlist_add_check"] = "\ue065";
        dict["PlaylistAddCheck"] = "\ue065";
        dict["playlist_add_check_circle"] = "\ue7e6";
        dict["PlaylistAddCheckCircle"] = "\ue7e6";
        dict["playlist_add_circle"] = "\ue7e5";
        dict["PlaylistAddCircle"] = "\ue7e5";
        dict["playlist_play"] = "\ue05f";
        dict["PlaylistPlay"] = "\ue05f";
        dict["playlist_remove"] = "\ueb80";
        dict["PlaylistRemove"] = "\ueb80";
        dict["plug_connect"] = "\uf35a";
        dict["PlugConnect"] = "\uf35a";
        dict["plumbing"] = "\uf107";
        dict["plus_one"] = "\ue800";
        dict["PlusOne"] = "\ue800";
        dict["podcasts"] = "\uf048";
        dict["podiatry"] = "\ue120";
        dict["podium"] = "\uf7fb";
        dict["point_of_sale"] = "\uf17e";
        dict["PointOfSale"] = "\uf17e";
        dict["point_scan"] = "\uf70c";
        dict["PointScan"] = "\uf70c";
        dict["poker_chip"] = "\uf49b";
        dict["PokerChip"] = "\uf49b";
        dict["policy"] = "\uea17";
        dict["policy_alert"] = "\uf407";
        dict["PolicyAlert"] = "\uf407";
        dict["poll"] = "\uf0cc";
        dict["polyline"] = "\uebbb";
        dict["polymer"] = "\ue8ab";
        dict["pool"] = "\ueb48";
        dict["portable_wifi_off"] = "\uf087";
        dict["PortableWifiOff"] = "\uf087";
        dict["portrait"] = "\ue851";
        dict["position_bottom_left"] = "\uf70b";
        dict["PositionBottomLeft"] = "\uf70b";
        dict["position_bottom_right"] = "\uf70a";
        dict["PositionBottomRight"] = "\uf70a";
        dict["position_top_right"] = "\uf709";
        dict["PositionTopRight"] = "\uf709";
        dict["post"] = "\ue705";
        dict["post_add"] = "\uea20";
        dict["PostAdd"] = "\uea20";
        dict["potted_plant"] = "\uf8aa";
        dict["PottedPlant"] = "\uf8aa";
        dict["power"] = "\ue63c";
        dict["power_input"] = "\ue336";
        dict["PowerInput"] = "\ue336";
        dict["power_off"] = "\ue646";
        dict["PowerOff"] = "\ue646";
        dict["power_rounded"] = "\uf8c7";
        dict["PowerRounded"] = "\uf8c7";
        dict["power_settings_circle"] = "\uf418";
        dict["PowerSettingsCircle"] = "\uf418";
        dict["power_settings_new"] = "\uf8c7";
        dict["PowerSettingsNew"] = "\uf8c7";
        dict["prayer_times"] = "\uf838";
        dict["PrayerTimes"] = "\uf838";
        dict["precision_manufacturing"] = "\uf049";
        dict["PrecisionManufacturing"] = "\uf049";
        dict["pregnancy"] = "\uf5f1";
        dict["pregnant_woman"] = "\uf5f1";
        dict["PregnantWoman"] = "\uf5f1";
        dict["preliminary"] = "\ue7d8";
        dict["prescriptions"] = "\ue121";
        dict["present_to_all"] = "\ue0df";
        dict["PresentToAll"] = "\ue0df";
        dict["preview"] = "\uf1c5";
        dict["preview_off"] = "\uf7af";
        dict["PreviewOff"] = "\uf7af";
        dict["price_change"] = "\uf04a";
        dict["PriceChange"] = "\uf04a";
        dict["price_check"] = "\uf04b";
        dict["PriceCheck"] = "\uf04b";
        dict["print"] = "\ue8ad";
        dict["print_add"] = "\uf7a2";
        dict["PrintAdd"] = "\uf7a2";
        dict["print_connect"] = "\uf7a1";
        dict["PrintConnect"] = "\uf7a1";
        dict["print_disabled"] = "\ue9cf";
        dict["PrintDisabled"] = "\ue9cf";
        dict["print_error"] = "\uf7a0";
        dict["PrintError"] = "\uf7a0";
        dict["print_lock"] = "\uf651";
        dict["PrintLock"] = "\uf651";
        dict["priority"] = "\uefb4";
        dict["priority_high"] = "\ue645";
        dict["PriorityHigh"] = "\ue645";
        dict["privacy"] = "\uf148";
        dict["privacy_tip"] = "\uf0dc";
        dict["PrivacyTip"] = "\uf0dc";
        dict["private_connectivity"] = "\ue744";
        dict["PrivateConnectivity"] = "\ue744";
        dict["problem"] = "\ue122";
        dict["procedure"] = "\ue651";
        dict["process_chart"] = "\uf855";
        dict["ProcessChart"] = "\uf855";
        dict["production_quantity_limits"] = "\ue1d1";
        dict["ProductionQuantityLimits"] = "\ue1d1";
        dict["productivity"] = "\ue296";
        dict["progress_activity"] = "\ue9d0";
        dict["ProgressActivity"] = "\ue9d0";
        dict["prompt_suggestion"] = "\uf4f6";
        dict["PromptSuggestion"] = "\uf4f6";
        dict["propane"] = "\uec14";
        dict["propane_tank"] = "\uec13";
        dict["PropaneTank"] = "\uec13";
        dict["psychiatry"] = "\ue123";
        dict["psychology"] = "\uea4a";
        dict["psychology_alt"] = "\uf8ea";
        dict["PsychologyAlt"] = "\uf8ea";
        dict["public"] = "\ue80b";
        dict["public_off"] = "\uf1ca";
        dict["PublicOff"] = "\uf1ca";
        dict["publish"] = "\ue255";
        dict["published_with_changes"] = "\uf232";
        dict["PublishedWithChanges"] = "\uf232";
        dict["pulmonology"] = "\ue124";
        dict["pulse_alert"] = "\uf501";
        dict["PulseAlert"] = "\uf501";
        dict["punch_clock"] = "\ueaa8";
        dict["PunchClock"] = "\ueaa8";
        dict["push_pin"] = "\uf10d";
        dict["PushPin"] = "\uf10d";
        dict["qr_code"] = "\uef6b";
        dict["QrCode"] = "\uef6b";
        dict["qr_code_2"] = "\ue00a";
        dict["QrCode2"] = "\ue00a";
        dict["qr_code_2_add"] = "\uf658";
        dict["QrCode2Add"] = "\uf658";
        dict["qr_code_scanner"] = "\uf206";
        dict["QrCodeScanner"] = "\uf206";
        dict["query_builder"] = "\uefd6";
        dict["QueryBuilder"] = "\uefd6";
        dict["query_stats"] = "\ue4fc";
        dict["QueryStats"] = "\ue4fc";
        dict["question_answer"] = "\ue8af";
        dict["QuestionAnswer"] = "\ue8af";
        dict["question_exchange"] = "\uf7f3";
        dict["QuestionExchange"] = "\uf7f3";
        dict["question_mark"] = "\ueb8b";
        dict["QuestionMark"] = "\ueb8b";
        dict["queue"] = "\ue03c";
        dict["queue_music"] = "\ue03d";
        dict["QueueMusic"] = "\ue03d";
        dict["queue_play_next"] = "\ue066";
        dict["QueuePlayNext"] = "\ue066";
        dict["quick_phrases"] = "\ue7d1";
        dict["QuickPhrases"] = "\ue7d1";
        dict["quick_reference"] = "\ue46e";
        dict["QuickReference"] = "\ue46e";
        dict["quick_reference_all"] = "\uf801";
        dict["QuickReferenceAll"] = "\uf801";
        dict["quick_reorder"] = "\ueb15";
        dict["QuickReorder"] = "\ueb15";
        dict["quickreply"] = "\uef6c";
        dict["quiet_time"] = "\uf159";
        dict["QuietTime"] = "\uf159";
        dict["quiet_time_active"] = "\ueb76";
        dict["QuietTimeActive"] = "\ueb76";
        dict["quiz"] = "\uf04c";
        dict["r_mobiledata"] = "\uf04d";
        dict["RMobiledata"] = "\uf04d";
        dict["radar"] = "\uf04e";
        dict["radio"] = "\ue03e";
        dict["radio_button_checked"] = "\ue837";
        dict["RadioButtonChecked"] = "\ue837";
        dict["radio_button_partial"] = "\uf560";
        dict["RadioButtonPartial"] = "\uf560";
        dict["radio_button_unchecked"] = "\ue836";
        dict["RadioButtonUnchecked"] = "\ue836";
        dict["radiology"] = "\ue125";
        dict["railway_alert"] = "\ue9d1";
        dict["RailwayAlert"] = "\ue9d1";
        dict["railway_alert_2"] = "\uf461";
        dict["RailwayAlert2"] = "\uf461";
        dict["rainy"] = "\uf176";
        dict["rainy_heavy"] = "\uf61f";
        dict["RainyHeavy"] = "\uf61f";
        dict["rainy_light"] = "\uf61e";
        dict["RainyLight"] = "\uf61e";
        dict["rainy_snow"] = "\uf61d";
        dict["RainySnow"] = "\uf61d";
        dict["ramen_dining"] = "\uea64";
        dict["RamenDining"] = "\uea64";
        dict["ramp_left"] = "\ueb9c";
        dict["RampLeft"] = "\ueb9c";
        dict["ramp_right"] = "\ueb96";
        dict["RampRight"] = "\ueb96";
        dict["range_hood"] = "\ue1ea";
        dict["RangeHood"] = "\ue1ea";
        dict["rate_review"] = "\ue560";
        dict["RateReview"] = "\ue560";
        dict["rate_review_rtl"] = "\ue706";
        dict["RateReviewRtl"] = "\ue706";
        dict["raven"] = "\uf555";
        dict["raw_off"] = "\uf04f";
        dict["RawOff"] = "\uf04f";
        dict["raw_on"] = "\uf050";
        dict["RawOn"] = "\uf050";
        dict["read_more"] = "\uef6d";
        dict["ReadMore"] = "\uef6d";
        dict["readiness_score"] = "\uf6dd";
        dict["ReadinessScore"] = "\uf6dd";
        dict["real_estate_agent"] = "\ue73a";
        dict["RealEstateAgent"] = "\ue73a";
        dict["rear_camera"] = "\uf6c2";
        dict["RearCamera"] = "\uf6c2";
        dict["rebase"] = "\uf845";
        dict["rebase_edit"] = "\uf846";
        dict["RebaseEdit"] = "\uf846";
        dict["receipt"] = "\ue8b0";
        dict["receipt_long"] = "\uef6e";
        dict["ReceiptLong"] = "\uef6e";
        dict["receipt_long_off"] = "\uf40a";
        dict["ReceiptLongOff"] = "\uf40a";
        dict["recent_actors"] = "\ue03f";
        dict["RecentActors"] = "\ue03f";
        dict["recent_patient"] = "\uf808";
        dict["RecentPatient"] = "\uf808";
        dict["recenter"] = "\uf4c0";
        dict["recommend"] = "\ue9d2";
        dict["record_voice_over"] = "\ue91f";
        dict["RecordVoiceOver"] = "\ue91f";
        dict["rectangle"] = "\ueb54";
        dict["rectangle_add"] = "\ueec8";
        dict["RectangleAdd"] = "\ueec8";
        dict["recycling"] = "\ue760";
        dict["redeem"] = "\ue8f6";
        dict["redo"] = "\ue15a";
        dict["reduce_capacity"] = "\uf21c";
        dict["ReduceCapacity"] = "\uf21c";
        dict["refresh"] = "\ue5d5";
        dict["regular_expression"] = "\uf750";
        dict["RegularExpression"] = "\uf750";
        dict["relax"] = "\uf6dc";
        dict["release_alert"] = "\uf654";
        dict["ReleaseAlert"] = "\uf654";
        dict["remember_me"] = "\uf051";
        dict["RememberMe"] = "\uf051";
        dict["reminder"] = "\ue6c6";
        dict["reminders_alt"] = "\ue6c6";
        dict["RemindersAlt"] = "\ue6c6";
        dict["remote_gen"] = "\ue83e";
        dict["RemoteGen"] = "\ue83e";
        dict["remove"] = "\ue15b";
        dict["remove_circle"] = "\uf08f";
        dict["RemoveCircle"] = "\uf08f";
        dict["remove_circle_outline"] = "\uf08f";
        dict["RemoveCircleOutline"] = "\uf08f";
        dict["remove_done"] = "\ue9d3";
        dict["RemoveDone"] = "\ue9d3";
        dict["remove_from_queue"] = "\ue067";
        dict["RemoveFromQueue"] = "\ue067";
        dict["remove_moderator"] = "\ue9d4";
        dict["RemoveModerator"] = "\ue9d4";
        dict["remove_red_eye"] = "\ue8f4";
        dict["RemoveRedEye"] = "\ue8f4";
        dict["remove_road"] = "\uebfc";
        dict["RemoveRoad"] = "\uebfc";
        dict["remove_selection"] = "\ue9d5";
        dict["RemoveSelection"] = "\ue9d5";
        dict["remove_shopping_cart"] = "\ue928";
        dict["RemoveShoppingCart"] = "\ue928";
        dict["reopen_window"] = "\uf708";
        dict["ReopenWindow"] = "\uf708";
        dict["reorder"] = "\ue8fe";
        dict["repartition"] = "\uf8e8";
        dict["repeat"] = "\ue040";
        dict["repeat_on"] = "\ue9d6";
        dict["RepeatOn"] = "\ue9d6";
        dict["repeat_one"] = "\ue041";
        dict["RepeatOne"] = "\ue041";
        dict["repeat_one_on"] = "\ue9d7";
        dict["RepeatOneOn"] = "\ue9d7";
        dict["replace_audio"] = "\uf451";
        dict["ReplaceAudio"] = "\uf451";
        dict["replace_image"] = "\uf450";
        dict["ReplaceImage"] = "\uf450";
        dict["replace_video"] = "\uf44f";
        dict["ReplaceVideo"] = "\uf44f";
        dict["replay"] = "\ue042";
        dict["replay_10"] = "\ue059";
        dict["Replay10"] = "\ue059";
        dict["replay_30"] = "\ue05a";
        dict["Replay30"] = "\ue05a";
        dict["replay_5"] = "\ue05b";
        dict["Replay5"] = "\ue05b";
        dict["replay_circle_filled"] = "\ue9d8";
        dict["ReplayCircleFilled"] = "\ue9d8";
        dict["reply"] = "\ue15e";
        dict["reply_all"] = "\ue15f";
        dict["ReplyAll"] = "\ue15f";
        dict["report"] = "\uf052";
        dict["report_gmailerrorred"] = "\uf052";
        dict["ReportGmailerrorred"] = "\uf052";
        dict["report_off"] = "\ue170";
        dict["ReportOff"] = "\ue170";
        dict["report_problem"] = "\uf083";
        dict["ReportProblem"] = "\uf083";
        dict["request_page"] = "\uf22c";
        dict["RequestPage"] = "\uf22c";
        dict["request_quote"] = "\uf1b6";
        dict["RequestQuote"] = "\uf1b6";
        dict["reset_brightness"] = "\uf482";
        dict["ResetBrightness"] = "\uf482";
        dict["reset_colors"] = "\U000FFEE2";
        dict["ResetColors"] = "\U000FFEE2";
        dict["reset_exposure"] = "\uf266";
        dict["ResetExposure"] = "\uf266";
        dict["reset_focus"] = "\uf481";
        dict["ResetFocus"] = "\uf481";
        dict["reset_image"] = "\uf824";
        dict["ResetImage"] = "\uf824";
        dict["reset_iso"] = "\uf480";
        dict["ResetIso"] = "\uf480";
        dict["reset_settings"] = "\uf47f";
        dict["ResetSettings"] = "\uf47f";
        dict["reset_shadow"] = "\uf47e";
        dict["ResetShadow"] = "\uf47e";
        dict["reset_shutter_speed"] = "\uf47d";
        dict["ResetShutterSpeed"] = "\uf47d";
        dict["reset_tv"] = "\ue9d9";
        dict["ResetTv"] = "\ue9d9";
        dict["reset_white_balance"] = "\uf47c";
        dict["ResetWhiteBalance"] = "\uf47c";
        dict["reset_wrench"] = "\uf56c";
        dict["ResetWrench"] = "\uf56c";
        dict["resize"] = "\uf707";
        dict["resize_window"] = "\U000FFF99";
        dict["ResizeWindow"] = "\U000FFF99";
        dict["respiratory_rate"] = "\ue127";
        dict["RespiratoryRate"] = "\ue127";
        dict["responsive_layout"] = "\ue9da";
        dict["ResponsiveLayout"] = "\ue9da";
        dict["rest_area"] = "\uf22a";
        dict["RestArea"] = "\uf22a";
        dict["restart_alt"] = "\uf053";
        dict["RestartAlt"] = "\uf053";
        dict["restaurant"] = "\ue56c";
        dict["restaurant_menu"] = "\ue561";
        dict["RestaurantMenu"] = "\ue561";
        dict["restore"] = "\ue8b3";
        dict["restore_from_trash"] = "\ue938";
        dict["RestoreFromTrash"] = "\ue938";
        dict["restore_page"] = "\ue929";
        dict["RestorePage"] = "\ue929";
        dict["resume"] = "\uf7d0";
        dict["reviews"] = "\uf07c";
        dict["rewarded_ads"] = "\uefb6";
        dict["RewardedAds"] = "\uefb6";
        dict["rheumatology"] = "\ue128";
        dict["rib_cage"] = "\uf898";
        dict["RibCage"] = "\uf898";
        dict["rice_bowl"] = "\uf1f5";
        dict["RiceBowl"] = "\uf1f5";
        dict["right_click"] = "\uf706";
        dict["RightClick"] = "\uf706";
        dict["right_panel_close"] = "\uf705";
        dict["RightPanelClose"] = "\uf705";
        dict["right_panel_open"] = "\uf704";
        dict["RightPanelOpen"] = "\uf704";
        dict["ring_volume"] = "\uf0dd";
        dict["RingVolume"] = "\uf0dd";
        dict["ring_volume_filled"] = "\uf0dd";
        dict["RingVolumeFilled"] = "\uf0dd";
        dict["ripples"] = "\ue9db";
        dict["road"] = "\uf472";
        dict["robot"] = "\uf882";
        dict["robot_2"] = "\uf5d0";
        dict["Robot2"] = "\uf5d0";
        dict["rocket"] = "\ueba5";
        dict["rocket_launch"] = "\ueb9b";
        dict["RocketLaunch"] = "\ueb9b";
        dict["roller_shades"] = "\uec12";
        dict["RollerShades"] = "\uec12";
        dict["roller_shades_closed"] = "\uec11";
        dict["RollerShadesClosed"] = "\uec11";
        dict["roller_skating"] = "\uebcd";
        dict["RollerSkating"] = "\uebcd";
        dict["roofing"] = "\uf201";
        dict["room"] = "\uf1db";
        dict["room_preferences"] = "\uf1b8";
        dict["RoomPreferences"] = "\uf1b8";
        dict["room_service"] = "\ueb49";
        dict["RoomService"] = "\ueb49";
        dict["rotate_90_degrees_ccw"] = "\ue418";
        dict["Rotate90DegreesCcw"] = "\ue418";
        dict["rotate_90_degrees_cw"] = "\ueaab";
        dict["Rotate90DegreesCw"] = "\ueaab";
        dict["rotate_auto"] = "\uf417";
        dict["RotateAuto"] = "\uf417";
        dict["rotate_left"] = "\ue419";
        dict["RotateLeft"] = "\ue419";
        dict["rotate_right"] = "\ue41a";
        dict["RotateRight"] = "\ue41a";
        dict["roundabout_left"] = "\ueb99";
        dict["RoundaboutLeft"] = "\ueb99";
        dict["roundabout_right"] = "\ueba3";
        dict["RoundaboutRight"] = "\ueba3";
        dict["rounded_corner"] = "\ue920";
        dict["RoundedCorner"] = "\ue920";
        dict["route"] = "\ueacd";
        dict["router"] = "\ue328";
        dict["router_off"] = "\uf2f4";
        dict["RouterOff"] = "\uf2f4";
        dict["routine"] = "\ue20c";
        dict["rowing"] = "\ue921";
        dict["rss_feed"] = "\ue0e5";
        dict["RssFeed"] = "\ue0e5";
        dict["rsvp"] = "\uf055";
        dict["rtt"] = "\ue9ad";
        dict["rubric"] = "\ueb27";
        dict["rule"] = "\uf1c2";
        dict["rule_folder"] = "\uf1c9";
        dict["RuleFolder"] = "\uf1c9";
        dict["rule_settings"] = "\uf64c";
        dict["RuleSettings"] = "\uf64c";
        dict["run_circle"] = "\uef6f";
        dict["RunCircle"] = "\uef6f";
        dict["running_with_errors"] = "\ue51d";
        dict["RunningWithErrors"] = "\ue51d";
        dict["rv_hookup"] = "\ue642";
        dict["RvHookup"] = "\ue642";
        dict["safety_check"] = "\uebef";
        dict["SafetyCheck"] = "\uebef";
        dict["safety_check_off"] = "\uf59d";
        dict["SafetyCheckOff"] = "\uf59d";
        dict["safety_divider"] = "\ue1cc";
        dict["SafetyDivider"] = "\ue1cc";
        dict["sailing"] = "\ue502";
        dict["salinity"] = "\uf876";
        dict["sanitizer"] = "\uf21d";
        dict["satellite"] = "\ue562";
        dict["satellite_alt"] = "\ueb3a";
        dict["SatelliteAlt"] = "\ueb3a";
        dict["sauna"] = "\uf6f7";
        dict["save"] = "\ue161";
        dict["save_alt"] = "\uf090";
        dict["SaveAlt"] = "\uf090";
        dict["save_as"] = "\ueb60";
        dict["SaveAs"] = "\ueb60";
        dict["save_clock"] = "\uf398";
        dict["SaveClock"] = "\uf398";
        dict["saved_search"] = "\uea11";
        dict["SavedSearch"] = "\uea11";
        dict["savings"] = "\ue2eb";
        dict["scale"] = "\ueb5f";
        dict["scan"] = "\uf74e";
        dict["scan_delete"] = "\uf74f";
        dict["ScanDelete"] = "\uf74f";
        dict["scanner"] = "\ue329";
        dict["scatter_plot"] = "\ue268";
        dict["ScatterPlot"] = "\ue268";
        dict["scene"] = "\ue2a7";
        dict["schedule"] = "\uefd6";
        dict["schedule_send"] = "\uea0a";
        dict["ScheduleSend"] = "\uea0a";
        dict["schema"] = "\ue4fd";
        dict["school"] = "\ue80c";
        dict["science"] = "\uea4b";
        dict["science_off"] = "\uf542";
        dict["ScienceOff"] = "\uf542";
        dict["scooter"] = "\uf471";
        dict["score"] = "\ue269";
        dict["scoreboard"] = "\uebd0";
        dict["screen_lock_landscape"] = "\uf2d8";
        dict["ScreenLockLandscape"] = "\uf2d8";
        dict["screen_lock_portrait"] = "\uf2be";
        dict["ScreenLockPortrait"] = "\uf2be";
        dict["screen_lock_rotation"] = "\uf2d6";
        dict["ScreenLockRotation"] = "\uf2d6";
        dict["screen_record"] = "\uf679";
        dict["ScreenRecord"] = "\uf679";
        dict["screen_rotation"] = "\uf2d5";
        dict["ScreenRotation"] = "\uf2d5";
        dict["screen_rotation_alt"] = "\uebee";
        dict["ScreenRotationAlt"] = "\uebee";
        dict["screen_rotation_up"] = "\uf678";
        dict["ScreenRotationUp"] = "\uf678";
        dict["screen_search_desktop"] = "\uef70";
        dict["ScreenSearchDesktop"] = "\uef70";
        dict["screen_share"] = "\ue0e2";
        dict["ScreenShare"] = "\ue0e2";
        dict["screenshot"] = "\uf056";
        dict["screenshot_frame"] = "\uf677";
        dict["ScreenshotFrame"] = "\uf677";
        dict["screenshot_frame_2"] = "\uf374";
        dict["ScreenshotFrame2"] = "\uf374";
        dict["screenshot_keyboard"] = "\uf7d3";
        dict["ScreenshotKeyboard"] = "\uf7d3";
        dict["screenshot_monitor"] = "\uec08";
        dict["ScreenshotMonitor"] = "\uec08";
        dict["screenshot_region"] = "\uf7d2";
        dict["ScreenshotRegion"] = "\uf7d2";
        dict["screenshot_tablet"] = "\uf697";
        dict["ScreenshotTablet"] = "\uf697";
        dict["script"] = "\uf45f";
        dict["scrollable_header"] = "\ue9dc";
        dict["ScrollableHeader"] = "\ue9dc";
        dict["scuba_diving"] = "\uebce";
        dict["ScubaDiving"] = "\uebce";
        dict["sd"] = "\ue9dd";
        dict["sd_card"] = "\ue623";
        dict["SdCard"] = "\ue623";
        dict["sd_card_alert"] = "\uf057";
        dict["SdCardAlert"] = "\uf057";
        dict["sd_storage"] = "\ue623";
        dict["SdStorage"] = "\ue623";
        dict["sdk"] = "\ue720";
        dict["search"] = "\uef7a";
        dict["search_activity"] = "\uf3e5";
        dict["SearchActivity"] = "\uf3e5";
        dict["search_check"] = "\uf800";
        dict["SearchCheck"] = "\uf800";
        dict["search_check_2"] = "\uf469";
        dict["SearchCheck2"] = "\uf469";
        dict["search_gear"] = "\ueefa";
        dict["SearchGear"] = "\ueefa";
        dict["search_hands_free"] = "\ue696";
        dict["SearchHandsFree"] = "\ue696";
        dict["search_insights"] = "\uf4bc";
        dict["SearchInsights"] = "\uf4bc";
        dict["search_off"] = "\uea76";
        dict["SearchOff"] = "\uea76";
        dict["seat_cool_left"] = "\uf331";
        dict["SeatCoolLeft"] = "\uf331";
        dict["seat_cool_right"] = "\uf330";
        dict["SeatCoolRight"] = "\uf330";
        dict["seat_heat_left"] = "\uf32f";
        dict["SeatHeatLeft"] = "\uf32f";
        dict["seat_heat_right"] = "\uf32e";
        dict["SeatHeatRight"] = "\uf32e";
        dict["seat_read"] = "\U000FFEDA";
        dict["SeatRead"] = "\U000FFEDA";
        dict["seat_vent_left"] = "\uf32d";
        dict["SeatVentLeft"] = "\uf32d";
        dict["seat_vent_right"] = "\uf32c";
        dict["SeatVentRight"] = "\uf32c";
        dict["seat_window"] = "\U000FFEE1";
        dict["SeatWindow"] = "\U000FFEE1";
        dict["security"] = "\ue32a";
        dict["security_key"] = "\uf503";
        dict["SecurityKey"] = "\uf503";
        dict["security_update"] = "\uf2cd";
        dict["SecurityUpdate"] = "\uf2cd";
        dict["security_update_good"] = "\uf073";
        dict["SecurityUpdateGood"] = "\uf073";
        dict["security_update_warning"] = "\uf2d3";
        dict["SecurityUpdateWarning"] = "\uf2d3";
        dict["segment"] = "\ue94b";
        dict["select"] = "\uf74d";
        dict["select_all"] = "\ue162";
        dict["SelectAll"] = "\ue162";
        dict["select_check_box"] = "\uf1fe";
        dict["SelectCheckBox"] = "\uf1fe";
        dict["select_to_speak"] = "\uf7cf";
        dict["SelectToSpeak"] = "\uf7cf";
        dict["select_window"] = "\ue6fa";
        dict["SelectWindow"] = "\ue6fa";
        dict["select_window_2"] = "\uf4c8";
        dict["SelectWindow2"] = "\uf4c8";
        dict["select_window_off"] = "\ue506";
        dict["SelectWindowOff"] = "\ue506";
        dict["self_care"] = "\uf86d";
        dict["SelfCare"] = "\uf86d";
        dict["self_improvement"] = "\uea78";
        dict["SelfImprovement"] = "\uea78";
        dict["sell"] = "\uf05b";
        dict["sell_cloud"] = "\U000FFF7B";
        dict["SellCloud"] = "\U000FFF7B";
        dict["send"] = "\ue163";
        dict["send_and_archive"] = "\uea0c";
        dict["SendAndArchive"] = "\uea0c";
        dict["send_money"] = "\ue8b7";
        dict["SendMoney"] = "\ue8b7";
        dict["send_time_extension"] = "\ueadb";
        dict["SendTimeExtension"] = "\ueadb";
        dict["send_to_mobile"] = "\uf2d2";
        dict["SendToMobile"] = "\uf2d2";
        dict["sensor_door"] = "\uf1b5";
        dict["SensorDoor"] = "\uf1b5";
        dict["sensor_occupied"] = "\uec10";
        dict["SensorOccupied"] = "\uec10";
        dict["sensor_window"] = "\uf1b4";
        dict["SensorWindow"] = "\uf1b4";
        dict["sensors"] = "\ue51e";
        dict["sensors_krx"] = "\uf556";
        dict["SensorsKrx"] = "\uf556";
        dict["sensors_krx_off"] = "\uf515";
        dict["SensorsKrxOff"] = "\uf515";
        dict["sensors_off"] = "\ue51f";
        dict["SensorsOff"] = "\ue51f";
        dict["sentiment_calm"] = "\uf6a7";
        dict["SentimentCalm"] = "\uf6a7";
        dict["sentiment_content"] = "\uf6a6";
        dict["SentimentContent"] = "\uf6a6";
        dict["sentiment_dissatisfied"] = "\ue811";
        dict["SentimentDissatisfied"] = "\ue811";
        dict["sentiment_excited"] = "\uf6a5";
        dict["SentimentExcited"] = "\uf6a5";
        dict["sentiment_extremely_dissatisfied"] = "\uf194";
        dict["SentimentExtremelyDissatisfied"] = "\uf194";
        dict["sentiment_frustrated"] = "\uf6a4";
        dict["SentimentFrustrated"] = "\uf6a4";
        dict["sentiment_neutral"] = "\ue812";
        dict["SentimentNeutral"] = "\ue812";
        dict["sentiment_sad"] = "\uf6a3";
        dict["SentimentSad"] = "\uf6a3";
        dict["sentiment_satisfied"] = "\ue813";
        dict["SentimentSatisfied"] = "\ue813";
        dict["sentiment_satisfied_alt"] = "\ue813";
        dict["SentimentSatisfiedAlt"] = "\ue813";
        dict["sentiment_stressed"] = "\uf6a2";
        dict["SentimentStressed"] = "\uf6a2";
        dict["sentiment_very_dissatisfied"] = "\ue814";
        dict["SentimentVeryDissatisfied"] = "\ue814";
        dict["sentiment_very_satisfied"] = "\ue815";
        dict["SentimentVerySatisfied"] = "\ue815";
        dict["sentiment_worried"] = "\uf6a1";
        dict["SentimentWorried"] = "\uf6a1";
        dict["serif"] = "\uf4ac";
        dict["server_person"] = "\uf3bd";
        dict["ServerPerson"] = "\uf3bd";
        dict["service_toolbox"] = "\ue717";
        dict["ServiceToolbox"] = "\ue717";
        dict["set_meal"] = "\uf1ea";
        dict["SetMeal"] = "\uf1ea";
        dict["settings"] = "\ue8b8";
        dict["settings_accessibility"] = "\uf05d";
        dict["SettingsAccessibility"] = "\uf05d";
        dict["settings_account_box"] = "\uf835";
        dict["SettingsAccountBox"] = "\uf835";
        dict["settings_alert"] = "\uf143";
        dict["SettingsAlert"] = "\uf143";
        dict["settings_applications"] = "\ue8b9";
        dict["SettingsApplications"] = "\ue8b9";
        dict["settings_b_roll"] = "\uf625";
        dict["SettingsBRoll"] = "\uf625";
        dict["settings_backup_restore"] = "\ue8ba";
        dict["SettingsBackupRestore"] = "\ue8ba";
        dict["settings_bluetooth"] = "\ue8bb";
        dict["SettingsBluetooth"] = "\ue8bb";
        dict["settings_brightness"] = "\ue8bd";
        dict["SettingsBrightness"] = "\ue8bd";
        dict["settings_cell"] = "\uf2d1";
        dict["SettingsCell"] = "\uf2d1";
        dict["settings_cinematic_blur"] = "\uf624";
        dict["SettingsCinematicBlur"] = "\uf624";
        dict["settings_ethernet"] = "\ue8be";
        dict["SettingsEthernet"] = "\ue8be";
        dict["settings_heart"] = "\uf522";
        dict["SettingsHeart"] = "\uf522";
        dict["settings_input_antenna"] = "\ue8bf";
        dict["SettingsInputAntenna"] = "\ue8bf";
        dict["settings_input_component"] = "\ue8c1";
        dict["SettingsInputComponent"] = "\ue8c1";
        dict["settings_input_composite"] = "\ue8c1";
        dict["SettingsInputComposite"] = "\ue8c1";
        dict["settings_input_hdmi"] = "\ue8c2";
        dict["SettingsInputHdmi"] = "\ue8c2";
        dict["settings_input_svideo"] = "\ue8c3";
        dict["SettingsInputSvideo"] = "\ue8c3";
        dict["settings_motion_mode"] = "\uf833";
        dict["SettingsMotionMode"] = "\uf833";
        dict["settings_night_sight"] = "\uf832";
        dict["SettingsNightSight"] = "\uf832";
        dict["settings_overscan"] = "\ue8c4";
        dict["SettingsOverscan"] = "\ue8c4";
        dict["settings_panorama"] = "\uf831";
        dict["SettingsPanorama"] = "\uf831";
        dict["settings_phone"] = "\ue8c5";
        dict["SettingsPhone"] = "\ue8c5";
        dict["settings_photo_camera"] = "\uf834";
        dict["SettingsPhotoCamera"] = "\uf834";
        dict["settings_power"] = "\ue8c6";
        dict["SettingsPower"] = "\ue8c6";
        dict["settings_remote"] = "\ue8c7";
        dict["SettingsRemote"] = "\ue8c7";
        dict["settings_screen"] = "\U000FFEDF";
        dict["SettingsScreen"] = "\U000FFEDF";
        dict["settings_seating"] = "\uef2d";
        dict["SettingsSeating"] = "\uef2d";
        dict["settings_slow_motion"] = "\uf623";
        dict["SettingsSlowMotion"] = "\uf623";
        dict["settings_suggest"] = "\uf05e";
        dict["SettingsSuggest"] = "\uf05e";
        dict["settings_system_daydream"] = "\ue1c3";
        dict["SettingsSystemDaydream"] = "\ue1c3";
        dict["settings_timelapse"] = "\uf622";
        dict["SettingsTimelapse"] = "\uf622";
        dict["settings_video_camera"] = "\uf621";
        dict["SettingsVideoCamera"] = "\uf621";
        dict["settings_voice"] = "\ue8c8";
        dict["SettingsVoice"] = "\ue8c8";
        dict["settop_component"] = "\ue2ac";
        dict["SettopComponent"] = "\ue2ac";
        dict["severe_cold"] = "\uebd3";
        dict["SevereCold"] = "\uebd3";
        dict["shades"] = "\U000FFF73";
        dict["shades_closed"] = "\U000FFF74";
        dict["ShadesClosed"] = "\U000FFF74";
        dict["shadow"] = "\ue9df";
        dict["shadow_add"] = "\uf584";
        dict["ShadowAdd"] = "\uf584";
        dict["shadow_minus"] = "\uf583";
        dict["ShadowMinus"] = "\uf583";
        dict["shape_line"] = "\uf8d3";
        dict["ShapeLine"] = "\uf8d3";
        dict["shape_recognition"] = "\ueb01";
        dict["ShapeRecognition"] = "\ueb01";
        dict["shapes"] = "\ue602";
        dict["share"] = "\ue80d";
        dict["share_eta"] = "\ue5f7";
        dict["ShareEta"] = "\ue5f7";
        dict["share_location"] = "\uf05f";
        dict["ShareLocation"] = "\uf05f";
        dict["share_off"] = "\uf6cb";
        dict["ShareOff"] = "\uf6cb";
        dict["share_reviews"] = "\uf8a4";
        dict["ShareReviews"] = "\uf8a4";
        dict["share_windows"] = "\uf613";
        dict["ShareWindows"] = "\uf613";
        dict["shaved_ice"] = "\uf225";
        dict["ShavedIce"] = "\uf225";
        dict["sheets_rtl"] = "\uf823";
        dict["SheetsRtl"] = "\uf823";
        dict["shelf_auto_hide"] = "\uf703";
        dict["ShelfAutoHide"] = "\uf703";
        dict["shelf_position"] = "\uf702";
        dict["ShelfPosition"] = "\uf702";
        dict["shelves"] = "\uf86e";
        dict["shield"] = "\ue9e0";
        dict["shield_card"] = "\U000FFF30";
        dict["ShieldCard"] = "\U000FFF30";
        dict["shield_lock"] = "\uf686";
        dict["ShieldLock"] = "\uf686";
        dict["shield_locked"] = "\uf592";
        dict["ShieldLocked"] = "\uf592";
        dict["shield_moon"] = "\ueaa9";
        dict["ShieldMoon"] = "\ueaa9";
        dict["shield_person"] = "\uf650";
        dict["ShieldPerson"] = "\uf650";
        dict["shield_question"] = "\uf529";
        dict["ShieldQuestion"] = "\uf529";
        dict["shield_radar"] = "\U000FFF2F";
        dict["ShieldRadar"] = "\U000FFF2F";
        dict["shield_toggle"] = "\uf2ad";
        dict["ShieldToggle"] = "\uf2ad";
        dict["shield_watch"] = "\uf30f";
        dict["ShieldWatch"] = "\uf30f";
        dict["shield_with_heart"] = "\ue78f";
        dict["ShieldWithHeart"] = "\ue78f";
        dict["shield_with_house"] = "\ue78d";
        dict["ShieldWithHouse"] = "\ue78d";
        dict["shift"] = "\ue5f2";
        dict["shift_lock"] = "\uf7ae";
        dict["ShiftLock"] = "\uf7ae";
        dict["shift_lock_off"] = "\uf483";
        dict["ShiftLockOff"] = "\uf483";
        dict["shoe_cleats"] = "\U000FFFB3";
        dict["ShoeCleats"] = "\U000FFFB3";
        dict["shop"] = "\ue8c9";
        dict["shop_2"] = "\ue8ca";
        dict["Shop2"] = "\ue8ca";
        dict["shop_two"] = "\ue8ca";
        dict["ShopTwo"] = "\ue8ca";
        dict["shopping_bag"] = "\uf1cc";
        dict["ShoppingBag"] = "\uf1cc";
        dict["shopping_bag_speed"] = "\uf39a";
        dict["ShoppingBagSpeed"] = "\uf39a";
        dict["shopping_basket"] = "\ue8cb";
        dict["ShoppingBasket"] = "\ue8cb";
        dict["shopping_cart"] = "\ue8cc";
        dict["ShoppingCart"] = "\ue8cc";
        dict["shopping_cart_checkout"] = "\ueb88";
        dict["ShoppingCartCheckout"] = "\ueb88";
        dict["shopping_cart_off"] = "\uf4f7";
        dict["ShoppingCartOff"] = "\uf4f7";
        dict["shoppingmode"] = "\uefb7";
        dict["short_stay"] = "\ue4d0";
        dict["ShortStay"] = "\ue4d0";
        dict["short_text"] = "\ue261";
        dict["ShortText"] = "\ue261";
        dict["shortcut"] = "\uf57a";
        dict["show_chart"] = "\ue6e1";
        dict["ShowChart"] = "\ue6e1";
        dict["shower"] = "\uf061";
        dict["shuffle"] = "\ue043";
        dict["shuffle_on"] = "\ue9e1";
        dict["ShuffleOn"] = "\ue9e1";
        dict["shutter_speed"] = "\ue43d";
        dict["ShutterSpeed"] = "\ue43d";
        dict["shutter_speed_add"] = "\uf57e";
        dict["ShutterSpeedAdd"] = "\uf57e";
        dict["shutter_speed_minus"] = "\uf57d";
        dict["ShutterSpeedMinus"] = "\uf57d";
        dict["sick"] = "\uf220";
        dict["side_navigation"] = "\ue9e2";
        dict["SideNavigation"] = "\ue9e2";
        dict["sign_language"] = "\uebe5";
        dict["SignLanguage"] = "\uebe5";
        dict["sign_language_2"] = "\uf258";
        dict["SignLanguage2"] = "\uf258";
        dict["sign_language_off"] = "\U000FFEE4";
        dict["SignLanguageOff"] = "\U000FFEE4";
        dict["signal_cellular_0_bar"] = "\uf0a8";
        dict["SignalCellular0Bar"] = "\uf0a8";
        dict["signal_cellular_1_bar"] = "\uf0a9";
        dict["SignalCellular1Bar"] = "\uf0a9";
        dict["signal_cellular_2_bar"] = "\uf0aa";
        dict["SignalCellular2Bar"] = "\uf0aa";
        dict["signal_cellular_3_bar"] = "\uf0ab";
        dict["SignalCellular3Bar"] = "\uf0ab";
        dict["signal_cellular_4_bar"] = "\ue1c8";
        dict["SignalCellular4Bar"] = "\ue1c8";
        dict["signal_cellular_add"] = "\uf7a9";
        dict["SignalCellularAdd"] = "\uf7a9";
        dict["signal_cellular_alt"] = "\ue202";
        dict["SignalCellularAlt"] = "\ue202";
        dict["signal_cellular_alt_1_bar"] = "\uebdf";
        dict["SignalCellularAlt1Bar"] = "\uebdf";
        dict["signal_cellular_alt_2_bar"] = "\uebe3";
        dict["SignalCellularAlt2Bar"] = "\uebe3";
        dict["signal_cellular_alt_off"] = "\U000FFF8A";
        dict["SignalCellularAltOff"] = "\U000FFF8A";
        dict["signal_cellular_connected_no_internet_0_bar"] = "\uf0ac";
        dict["SignalCellularConnectedNoInternet0Bar"] = "\uf0ac";
        dict["signal_cellular_connected_no_internet_4_bar"] = "\ue1cd";
        dict["SignalCellularConnectedNoInternet4Bar"] = "\ue1cd";
        dict["signal_cellular_no_sim"] = "\ue1ce";
        dict["SignalCellularNoSim"] = "\ue1ce";
        dict["signal_cellular_nodata"] = "\uf062";
        dict["SignalCellularNodata"] = "\uf062";
        dict["signal_cellular_null"] = "\ue1cf";
        dict["SignalCellularNull"] = "\ue1cf";
        dict["signal_cellular_off"] = "\ue1d0";
        dict["SignalCellularOff"] = "\ue1d0";
        dict["signal_cellular_pause"] = "\uf5a7";
        dict["SignalCellularPause"] = "\uf5a7";
        dict["signal_disconnected"] = "\uf239";
        dict["SignalDisconnected"] = "\uf239";
        dict["signal_wifi_0_bar"] = "\uf0b0";
        dict["SignalWifi0Bar"] = "\uf0b0";
        dict["signal_wifi_4_bar"] = "\uf065";
        dict["SignalWifi4Bar"] = "\uf065";
        dict["signal_wifi_4_bar_lock"] = "\ue1e1";
        dict["SignalWifi4BarLock"] = "\ue1e1";
        dict["signal_wifi_bad"] = "\uf064";
        dict["SignalWifiBad"] = "\uf064";
        dict["signal_wifi_connected_no_internet_4"] = "\uf064";
        dict["SignalWifiConnectedNoInternet4"] = "\uf064";
        dict["signal_wifi_off"] = "\ue1da";
        dict["SignalWifiOff"] = "\ue1da";
        dict["signal_wifi_statusbar_4_bar"] = "\uf065";
        dict["SignalWifiStatusbar4Bar"] = "\uf065";
        dict["signal_wifi_statusbar_not_connected"] = "\uf0ef";
        dict["SignalWifiStatusbarNotConnected"] = "\uf0ef";
        dict["signal_wifi_statusbar_null"] = "\uf067";
        dict["SignalWifiStatusbarNull"] = "\uf067";
        dict["signature"] = "\uf74c";
        dict["signpost"] = "\ueb91";
        dict["sim_card"] = "\ue32b";
        dict["SimCard"] = "\ue32b";
        dict["sim_card_alert"] = "\uf057";
        dict["SimCardAlert"] = "\uf057";
        dict["sim_card_download"] = "\uf068";
        dict["SimCardDownload"] = "\uf068";
        dict["sim_card_lock"] = "\U000FFEC1";
        dict["SimCardLock"] = "\U000FFEC1";
        dict["simulation"] = "\uf3e1";
        dict["single_arrow"] = "\U000FFED9";
        dict["SingleArrow"] = "\U000FFED9";
        dict["single_bed"] = "\uea48";
        dict["SingleBed"] = "\uea48";
        dict["sip"] = "\uf069";
        dict["siren"] = "\uf3a7";
        dict["siren_check"] = "\uf3a6";
        dict["SirenCheck"] = "\uf3a6";
        dict["siren_open"] = "\uf3a5";
        dict["SirenOpen"] = "\uf3a5";
        dict["siren_question"] = "\uf3a4";
        dict["SirenQuestion"] = "\uf3a4";
        dict["skateboarding"] = "\ue511";
        dict["skeleton"] = "\uf899";
        dict["skillet"] = "\uf543";
        dict["skillet_cooktop"] = "\uf544";
        dict["SkilletCooktop"] = "\uf544";
        dict["skip_next"] = "\ue044";
        dict["SkipNext"] = "\ue044";
        dict["skip_previous"] = "\ue045";
        dict["SkipPrevious"] = "\ue045";
        dict["skull"] = "\uf89a";
        dict["skull_list"] = "\uf370";
        dict["SkullList"] = "\uf370";
        dict["slab_serif"] = "\uf4ab";
        dict["SlabSerif"] = "\uf4ab";
        dict["sledding"] = "\ue512";
        dict["sleep"] = "\ue213";
        dict["sleep_score"] = "\uf6b7";
        dict["SleepScore"] = "\uf6b7";
        dict["slide_library"] = "\uf822";
        dict["SlideLibrary"] = "\uf822";
        dict["sliders"] = "\ue9e3";
        dict["slideshow"] = "\ue41b";
        dict["slow_motion_video"] = "\ue068";
        dict["SlowMotionVideo"] = "\ue068";
        dict["smart_button"] = "\uf1c1";
        dict["SmartButton"] = "\uf1c1";
        dict["smart_card_reader"] = "\uf4a5";
        dict["SmartCardReader"] = "\uf4a5";
        dict["smart_card_reader_off"] = "\uf4a6";
        dict["SmartCardReaderOff"] = "\uf4a6";
        dict["smart_display"] = "\uf06a";
        dict["SmartDisplay"] = "\uf06a";
        dict["smart_outlet"] = "\ue844";
        dict["SmartOutlet"] = "\ue844";
        dict["smart_screen"] = "\uf2d0";
        dict["SmartScreen"] = "\uf2d0";
        dict["smart_toy"] = "\uf06c";
        dict["SmartToy"] = "\uf06c";
        dict["smartphone"] = "\ue7ba";
        dict["smartphone_camera"] = "\uf44e";
        dict["SmartphoneCamera"] = "\uf44e";
        dict["smb_share"] = "\uf74b";
        dict["SmbShare"] = "\uf74b";
        dict["smoke_free"] = "\ueb4a";
        dict["SmokeFree"] = "\ueb4a";
        dict["smoking_rooms"] = "\ueb4b";
        dict["SmokingRooms"] = "\ueb4b";
        dict["sms"] = "\ue625";
        dict["sms_failed"] = "\ue87f";
        dict["SmsFailed"] = "\ue87f";
        dict["snail"] = "\U000FFEDE";
        dict["snippet_folder"] = "\uf1c7";
        dict["SnippetFolder"] = "\uf1c7";
        dict["snooze"] = "\ue046";
        dict["snowboarding"] = "\ue513";
        dict["snowflake"] = "\ued5b";
        dict["snowing"] = "\ue80f";
        dict["snowing_heavy"] = "\uf61c";
        dict["SnowingHeavy"] = "\uf61c";
        dict["snowmobile"] = "\ue503";
        dict["snowshoeing"] = "\ue514";
        dict["soap"] = "\uf1b2";
        dict["soba"] = "\uef36";
        dict["social_distance"] = "\ue1cb";
        dict["SocialDistance"] = "\ue1cb";
        dict["social_leaderboard"] = "\uf6a0";
        dict["SocialLeaderboard"] = "\uf6a0";
        dict["solar_power"] = "\uec0f";
        dict["SolarPower"] = "\uec0f";
        dict["solo_dining"] = "\uef35";
        dict["SoloDining"] = "\uef35";
        dict["sort"] = "\ue164";
        dict["sort_by_alpha"] = "\ue053";
        dict["SortByAlpha"] = "\ue053";
        dict["sos"] = "\uebf7";
        dict["sound_detection_dog_barking"] = "\uf149";
        dict["SoundDetectionDogBarking"] = "\uf149";
        dict["sound_detection_glass_break"] = "\uf14a";
        dict["SoundDetectionGlassBreak"] = "\uf14a";
        dict["sound_detection_loud_sound"] = "\uf14b";
        dict["SoundDetectionLoudSound"] = "\uf14b";
        dict["sound_sampler"] = "\uf6b4";
        dict["SoundSampler"] = "\uf6b4";
        dict["soundbar"] = "\U000FFF72";
        dict["soup_kitchen"] = "\ue7d3";
        dict["SoupKitchen"] = "\ue7d3";
        dict["source"] = "\uf1c8";
        dict["source_environment"] = "\ue527";
        dict["SourceEnvironment"] = "\ue527";
        dict["source_notes"] = "\ue12d";
        dict["SourceNotes"] = "\ue12d";
        dict["south"] = "\uf1e3";
        dict["south_america"] = "\ue7e4";
        dict["SouthAmerica"] = "\ue7e4";
        dict["south_east"] = "\uf1e4";
        dict["SouthEast"] = "\uf1e4";
        dict["south_west"] = "\uf1e5";
        dict["SouthWest"] = "\uf1e5";
        dict["spa"] = "\ueb4c";
        dict["space_bar"] = "\ue256";
        dict["SpaceBar"] = "\ue256";
        dict["space_dashboard"] = "\ue66b";
        dict["SpaceDashboard"] = "\ue66b";
        dict["space_dashboard_2"] = "\U000FFF8C";
        dict["SpaceDashboard2"] = "\U000FFF8C";
        dict["spatial_audio"] = "\uebeb";
        dict["SpatialAudio"] = "\uebeb";
        dict["spatial_audio_off"] = "\uebe8";
        dict["SpatialAudioOff"] = "\uebe8";
        dict["spatial_gallery"] = "\U000FFEB6";
        dict["SpatialGallery"] = "\U000FFEB6";
        dict["spatial_speaker"] = "\uf4cf";
        dict["SpatialSpeaker"] = "\uf4cf";
        dict["spatial_tracking"] = "\uebea";
        dict["SpatialTracking"] = "\uebea";
        dict["speaker"] = "\ue32d";
        dict["speaker_2"] = "\U000FFF71";
        dict["Speaker2"] = "\U000FFF71";
        dict["speaker_3"] = "\U000FFEB7";
        dict["Speaker3"] = "\U000FFEB7";
        dict["speaker_group"] = "\ue32e";
        dict["SpeakerGroup"] = "\ue32e";
        dict["speaker_notes"] = "\ue8cd";
        dict["SpeakerNotes"] = "\ue8cd";
        dict["speaker_notes_off"] = "\ue92a";
        dict["SpeakerNotesOff"] = "\ue92a";
        dict["speaker_phone"] = "\ue0d2";
        dict["SpeakerPhone"] = "\ue0d2";
        dict["special_character"] = "\uf74a";
        dict["SpecialCharacter"] = "\uf74a";
        dict["specific_gravity"] = "\uf872";
        dict["SpecificGravity"] = "\uf872";
        dict["speech_to_text"] = "\uf8a7";
        dict["SpeechToText"] = "\uf8a7";
        dict["speech_to_text_2"] = "\U000FFEC9";
        dict["SpeechToText2"] = "\U000FFEC9";
        dict["speed"] = "\ue9e4";
        dict["speed_0_25"] = "\uf4d4";
        dict["Speed025"] = "\uf4d4";
        dict["speed_0_2x"] = "\uf498";
        dict["Speed02x"] = "\uf498";
        dict["speed_0_5"] = "\uf4e2";
        dict["Speed05"] = "\uf4e2";
        dict["speed_0_5x"] = "\uf497";
        dict["Speed05x"] = "\uf497";
        dict["speed_0_75"] = "\uf4d3";
        dict["Speed075"] = "\uf4d3";
        dict["speed_0_7x"] = "\uf496";
        dict["Speed07x"] = "\uf496";
        dict["speed_1_2"] = "\uf4e1";
        dict["Speed12"] = "\uf4e1";
        dict["speed_1_25"] = "\uf4d2";
        dict["Speed125"] = "\uf4d2";
        dict["speed_1_2x"] = "\uf495";
        dict["Speed12x"] = "\uf495";
        dict["speed_1_5"] = "\uf4e0";
        dict["Speed15"] = "\uf4e0";
        dict["speed_1_5x"] = "\uf494";
        dict["Speed15x"] = "\uf494";
        dict["speed_1_75"] = "\uf4d1";
        dict["Speed175"] = "\uf4d1";
        dict["speed_1_7x"] = "\uf493";
        dict["Speed17x"] = "\uf493";
        dict["speed_2"] = "\U000FFF38";
        dict["Speed2"] = "\U000FFF38";
        dict["speed_2x"] = "\uf4eb";
        dict["Speed2x"] = "\uf4eb";
        dict["speed_3"] = "\U000FFF37";
        dict["Speed3"] = "\U000FFF37";
        dict["speed_4"] = "\U000FFF36";
        dict["Speed4"] = "\U000FFF36";
        dict["speed_camera"] = "\uf470";
        dict["SpeedCamera"] = "\uf470";
        dict["spellcheck"] = "\ue8ce";
        dict["split_scene"] = "\uf3bf";
        dict["SplitScene"] = "\uf3bf";
        dict["split_scene_2"] = "\U000FFEF7";
        dict["SplitScene2"] = "\U000FFEF7";
        dict["split_scene_down"] = "\uf2ff";
        dict["SplitSceneDown"] = "\uf2ff";
        dict["split_scene_left"] = "\uf2fe";
        dict["SplitSceneLeft"] = "\uf2fe";
        dict["split_scene_right"] = "\uf2fd";
        dict["SplitSceneRight"] = "\uf2fd";
        dict["split_scene_up"] = "\uf2fc";
        dict["SplitSceneUp"] = "\uf2fc";
        dict["splitscreen"] = "\uf06d";
        dict["splitscreen_add"] = "\uf4fd";
        dict["SplitscreenAdd"] = "\uf4fd";
        dict["splitscreen_bottom"] = "\uf676";
        dict["SplitscreenBottom"] = "\uf676";
        dict["splitscreen_landscape"] = "\uf459";
        dict["SplitscreenLandscape"] = "\uf459";
        dict["splitscreen_landscape_add"] = "\U000FFFBA";
        dict["SplitscreenLandscapeAdd"] = "\U000FFFBA";
        dict["splitscreen_left"] = "\uf675";
        dict["SplitscreenLeft"] = "\uf675";
        dict["splitscreen_portrait"] = "\uf458";
        dict["SplitscreenPortrait"] = "\uf458";
        dict["splitscreen_right"] = "\uf674";
        dict["SplitscreenRight"] = "\uf674";
        dict["splitscreen_top"] = "\uf673";
        dict["SplitscreenTop"] = "\uf673";
        dict["splitscreen_vertical_add"] = "\uf4fc";
        dict["SplitscreenVerticalAdd"] = "\uf4fc";
        dict["spo2"] = "\uf6db";
        dict["spoke"] = "\ue9a7";
        dict["sports"] = "\uea30";
        dict["sports_and_outdoors"] = "\uefb8";
        dict["SportsAndOutdoors"] = "\uefb8";
        dict["sports_bar"] = "\uf1f3";
        dict["SportsBar"] = "\uf1f3";
        dict["sports_baseball"] = "\uea51";
        dict["SportsBaseball"] = "\uea51";
        dict["sports_basketball"] = "\uea26";
        dict["SportsBasketball"] = "\uea26";
        dict["sports_cricket"] = "\uea27";
        dict["SportsCricket"] = "\uea27";
        dict["sports_esports"] = "\uea28";
        dict["SportsEsports"] = "\uea28";
        dict["sports_football"] = "\uea29";
        dict["SportsFootball"] = "\uea29";
        dict["sports_golf"] = "\uea2a";
        dict["SportsGolf"] = "\uea2a";
        dict["sports_gymnastics"] = "\uebc4";
        dict["SportsGymnastics"] = "\uebc4";
        dict["sports_handball"] = "\uea33";
        dict["SportsHandball"] = "\uea33";
        dict["sports_hockey"] = "\uea2b";
        dict["SportsHockey"] = "\uea2b";
        dict["sports_kabaddi"] = "\uea34";
        dict["SportsKabaddi"] = "\uea34";
        dict["sports_martial_arts"] = "\ueae9";
        dict["SportsMartialArts"] = "\ueae9";
        dict["sports_mma"] = "\uea2c";
        dict["SportsMma"] = "\uea2c";
        dict["sports_motorsports"] = "\uea2d";
        dict["SportsMotorsports"] = "\uea2d";
        dict["sports_rugby"] = "\uea2e";
        dict["SportsRugby"] = "\uea2e";
        dict["sports_score"] = "\uf06e";
        dict["SportsScore"] = "\uf06e";
        dict["sports_soccer"] = "\uea2f";
        dict["SportsSoccer"] = "\uea2f";
        dict["sports_tennis"] = "\uea32";
        dict["SportsTennis"] = "\uea32";
        dict["sports_volleyball"] = "\uea31";
        dict["SportsVolleyball"] = "\uea31";
        dict["sprinkler"] = "\ue29a";
        dict["sprint"] = "\uf81f";
        dict["sql"] = "\U000FFF95";
        dict["square"] = "\ueb36";
        dict["square_circle"] = "\ueec7";
        dict["SquareCircle"] = "\ueec7";
        dict["square_dot"] = "\uf3b3";
        dict["SquareDot"] = "\uf3b3";
        dict["square_foot"] = "\uea49";
        dict["SquareFoot"] = "\uea49";
        dict["ssid_chart"] = "\ueb66";
        dict["SsidChart"] = "\ueb66";
        dict["stack"] = "\uf609";
        dict["stack_group"] = "\uf359";
        dict["StackGroup"] = "\uf359";
        dict["stack_hexagon"] = "\uf41c";
        dict["StackHexagon"] = "\uf41c";
        dict["stack_off"] = "\uf608";
        dict["StackOff"] = "\uf608";
        dict["stack_star"] = "\uf607";
        dict["StackStar"] = "\uf607";
        dict["stacked_bar_chart"] = "\ue9e6";
        dict["StackedBarChart"] = "\ue9e6";
        dict["stacked_email"] = "\ue6c7";
        dict["StackedEmail"] = "\ue6c7";
        dict["stacked_inbox"] = "\ue6c9";
        dict["StackedInbox"] = "\ue6c9";
        dict["stacked_line_chart"] = "\uf22b";
        dict["StackedLineChart"] = "\uf22b";
        dict["stacks"] = "\uf500";
        dict["stadia_controller"] = "\uf135";
        dict["StadiaController"] = "\uf135";
        dict["stadium"] = "\ueb90";
        dict["stairs"] = "\uf1a9";
        dict["stairs_2"] = "\uf46c";
        dict["Stairs2"] = "\uf46c";
        dict["star"] = "\uf09a";
        dict["star_border"] = "\uf09a";
        dict["StarBorder"] = "\uf09a";
        dict["star_border_purple500"] = "\uf09a";
        dict["StarBorderPurple500"] = "\uf09a";
        dict["star_half"] = "\ue839";
        dict["StarHalf"] = "\ue839";
        dict["star_outline"] = "\uf09a";
        dict["StarOutline"] = "\uf09a";
        dict["star_purple500"] = "\uf09a";
        dict["StarPurple500"] = "\uf09a";
        dict["star_rate"] = "\uf0ec";
        dict["StarRate"] = "\uf0ec";
        dict["star_rate_half"] = "\uec45";
        dict["StarRateHalf"] = "\uec45";
        dict["star_shine"] = "\uf31d";
        dict["StarShine"] = "\uf31d";
        dict["stars"] = "\ue8d0";
        dict["stars_2"] = "\uf31c";
        dict["Stars2"] = "\uf31c";
        dict["start"] = "\ue089";
        dict["stat_0"] = "\ue697";
        dict["Stat0"] = "\ue697";
        dict["stat_1"] = "\ue698";
        dict["Stat1"] = "\ue698";
        dict["stat_2"] = "\ue699";
        dict["Stat2"] = "\ue699";
        dict["stat_3"] = "\ue69a";
        dict["Stat3"] = "\ue69a";
        dict["stat_minus_1"] = "\ue69b";
        dict["StatMinus1"] = "\ue69b";
        dict["stat_minus_2"] = "\ue69c";
        dict["StatMinus2"] = "\ue69c";
        dict["stat_minus_3"] = "\ue69d";
        dict["StatMinus3"] = "\ue69d";
        dict["stay_current_landscape"] = "\ued3e";
        dict["StayCurrentLandscape"] = "\ued3e";
        dict["stay_current_portrait"] = "\ue7ba";
        dict["StayCurrentPortrait"] = "\ue7ba";
        dict["stay_primary_landscape"] = "\ued3e";
        dict["StayPrimaryLandscape"] = "\ued3e";
        dict["stay_primary_portrait"] = "\uf2d3";
        dict["StayPrimaryPortrait"] = "\uf2d3";
        dict["steering_wheel_cool"] = "\U000FFEBD";
        dict["SteeringWheelCool"] = "\U000FFEBD";
        dict["steering_wheel_heat"] = "\uf32b";
        dict["SteeringWheelHeat"] = "\uf32b";
        dict["step"] = "\uf6fe";
        dict["step_into"] = "\uf701";
        dict["StepInto"] = "\uf701";
        dict["step_out"] = "\uf700";
        dict["StepOut"] = "\uf700";
        dict["step_over"] = "\uf6ff";
        dict["StepOver"] = "\uf6ff";
        dict["steppers"] = "\ue9e7";
        dict["steps"] = "\uf6da";
        dict["stethoscope"] = "\uf805";
        dict["stethoscope_arrow"] = "\uf807";
        dict["StethoscopeArrow"] = "\uf807";
        dict["stethoscope_check"] = "\uf806";
        dict["StethoscopeCheck"] = "\uf806";
        dict["sticker"] = "\ue707";
        dict["sticker_add"] = "\ueec2";
        dict["StickerAdd"] = "\ueec2";
        dict["sticky_note"] = "\ue9e8";
        dict["StickyNote"] = "\ue9e8";
        dict["sticky_note_2"] = "\uf1fc";
        dict["StickyNote2"] = "\uf1fc";
        dict["stock_media"] = "\uf570";
        dict["StockMedia"] = "\uf570";
        dict["stockpot"] = "\uf545";
        dict["stop"] = "\ue047";
        dict["stop_circle"] = "\uef71";
        dict["StopCircle"] = "\uef71";
        dict["stop_screen_share"] = "\ue0e3";
        dict["StopScreenShare"] = "\ue0e3";
        dict["storage"] = "\ue1db";
        dict["store"] = "\ue8d1";
        dict["store_mall_directory"] = "\ue8d1";
        dict["StoreMallDirectory"] = "\ue8d1";
        dict["storefront"] = "\uea12";
        dict["storm"] = "\uf070";
        dict["straight"] = "\ueb95";
        dict["straighten"] = "\ue41c";
        dict["strategy"] = "\uf5df";
        dict["stream"] = "\ue9e9";
        dict["stream_apps"] = "\uf79f";
        dict["StreamApps"] = "\uf79f";
        dict["streetview"] = "\ue56e";
        dict["stress_management"] = "\uf6d9";
        dict["StressManagement"] = "\uf6d9";
        dict["strikethrough_s"] = "\ue257";
        dict["StrikethroughS"] = "\ue257";
        dict["stroke_full"] = "\uf749";
        dict["StrokeFull"] = "\uf749";
        dict["stroke_partial"] = "\uf748";
        dict["StrokePartial"] = "\uf748";
        dict["stroller"] = "\uf1ae";
        dict["style"] = "\ue41d";
        dict["styler"] = "\ue273";
        dict["stylus"] = "\uf604";
        dict["stylus_brush"] = "\uf366";
        dict["StylusBrush"] = "\uf366";
        dict["stylus_fountain_pen"] = "\uf365";
        dict["StylusFountainPen"] = "\uf365";
        dict["stylus_highlighter"] = "\uf364";
        dict["StylusHighlighter"] = "\uf364";
        dict["stylus_laser_pointer"] = "\uf747";
        dict["StylusLaserPointer"] = "\uf747";
        dict["stylus_note"] = "\uf603";
        dict["StylusNote"] = "\uf603";
        dict["stylus_pen"] = "\uf363";
        dict["StylusPen"] = "\uf363";
        dict["stylus_pencil"] = "\uf362";
        dict["StylusPencil"] = "\uf362";
        dict["subdirectory_arrow_left"] = "\ue5d9";
        dict["SubdirectoryArrowLeft"] = "\ue5d9";
        dict["subdirectory_arrow_right"] = "\ue5da";
        dict["SubdirectoryArrowRight"] = "\ue5da";
        dict["subheader"] = "\ue9ea";
        dict["subject"] = "\ue8d2";
        dict["subscript"] = "\uf111";
        dict["subscriptions"] = "\ue064";
        dict["subtitles"] = "\ue048";
        dict["subtitles_gear"] = "\uf355";
        dict["SubtitlesGear"] = "\uf355";
        dict["subtitles_off"] = "\uef72";
        dict["SubtitlesOff"] = "\uef72";
        dict["subway"] = "\ue56f";
        dict["subway_walk"] = "\uf287";
        dict["SubwayWalk"] = "\uf287";
        dict["subwoofer"] = "\U000FFF70";
        dict["summarize"] = "\uf071";
        dict["sunny"] = "\ue81a";
        dict["sunny_snowing"] = "\ue819";
        dict["SunnySnowing"] = "\ue819";
        dict["superscript"] = "\uf112";
        dict["supervised_user_circle"] = "\ue939";
        dict["SupervisedUserCircle"] = "\ue939";
        dict["supervised_user_circle_off"] = "\uf60e";
        dict["SupervisedUserCircleOff"] = "\uf60e";
        dict["supervisor_account"] = "\ue8d3";
        dict["SupervisorAccount"] = "\ue8d3";
        dict["support"] = "\uef73";
        dict["support_agent"] = "\uf0e2";
        dict["SupportAgent"] = "\uf0e2";
        dict["surfing"] = "\ue515";
        dict["surgical"] = "\ue131";
        dict["surround_sound"] = "\ue049";
        dict["SurroundSound"] = "\ue049";
        dict["swap_calls"] = "\ue0d7";
        dict["SwapCalls"] = "\ue0d7";
        dict["swap_driving_apps"] = "\ue69e";
        dict["SwapDrivingApps"] = "\ue69e";
        dict["swap_driving_apps_wheel"] = "\ue69f";
        dict["SwapDrivingAppsWheel"] = "\ue69f";
        dict["swap_horiz"] = "\ue8d4";
        dict["SwapHoriz"] = "\ue8d4";
        dict["swap_horizontal_circle"] = "\ue933";
        dict["SwapHorizontalCircle"] = "\ue933";
        dict["swap_vert"] = "\ue8d5";
        dict["SwapVert"] = "\ue8d5";
        dict["swap_vertical_circle"] = "\ue8d6";
        dict["SwapVerticalCircle"] = "\ue8d6";
        dict["sweep"] = "\ue6ac";
        dict["swipe"] = "\ue9ec";
        dict["swipe_down"] = "\ueb53";
        dict["SwipeDown"] = "\ueb53";
        dict["swipe_down_alt"] = "\ueb30";
        dict["SwipeDownAlt"] = "\ueb30";
        dict["swipe_left"] = "\ueb59";
        dict["SwipeLeft"] = "\ueb59";
        dict["swipe_left_2"] = "\U000FFF94";
        dict["SwipeLeft2"] = "\U000FFF94";
        dict["swipe_left_alt"] = "\ueb33";
        dict["SwipeLeftAlt"] = "\ueb33";
        dict["swipe_right"] = "\ueb52";
        dict["SwipeRight"] = "\ueb52";
        dict["swipe_right_2"] = "\U000FFF93";
        dict["SwipeRight2"] = "\U000FFF93";
        dict["swipe_right_alt"] = "\ueb56";
        dict["SwipeRightAlt"] = "\ueb56";
        dict["swipe_up"] = "\ueb2e";
        dict["SwipeUp"] = "\ueb2e";
        dict["swipe_up_alt"] = "\ueb35";
        dict["SwipeUpAlt"] = "\ueb35";
        dict["swipe_vertical"] = "\ueb51";
        dict["SwipeVertical"] = "\ueb51";
        dict["switch"] = "\ue1f4";
        dict["switch_access"] = "\uf6fd";
        dict["SwitchAccess"] = "\uf6fd";
        dict["switch_access_2"] = "\uf506";
        dict["SwitchAccess2"] = "\uf506";
        dict["switch_access_3"] = "\uf34d";
        dict["SwitchAccess3"] = "\uf34d";
        dict["switch_access_shortcut"] = "\ue7e1";
        dict["SwitchAccessShortcut"] = "\ue7e1";
        dict["switch_access_shortcut_add"] = "\ue7e2";
        dict["SwitchAccessShortcutAdd"] = "\ue7e2";
        dict["switch_account"] = "\ue9ed";
        dict["SwitchAccount"] = "\ue9ed";
        dict["switch_camera"] = "\ue41e";
        dict["SwitchCamera"] = "\ue41e";
        dict["switch_left"] = "\uf1d1";
        dict["SwitchLeft"] = "\uf1d1";
        dict["switch_off"] = "\U000FFF6F";
        dict["SwitchOff"] = "\U000FFF6F";
        dict["switch_right"] = "\uf1d2";
        dict["SwitchRight"] = "\uf1d2";
        dict["switch_video"] = "\ue41f";
        dict["SwitchVideo"] = "\ue41f";
        dict["switches"] = "\ue733";
        dict["sword_rose"] = "\uf5de";
        dict["SwordRose"] = "\uf5de";
        dict["swords"] = "\uf889";
        dict["symptoms"] = "\ue132";
        dict["synagogue"] = "\ueab0";
        dict["sync"] = "\ue627";
        dict["sync_alt"] = "\uea18";
        dict["SyncAlt"] = "\uea18";
        dict["sync_arrow_down"] = "\uf37c";
        dict["SyncArrowDown"] = "\uf37c";
        dict["sync_arrow_up"] = "\uf37b";
        dict["SyncArrowUp"] = "\uf37b";
        dict["sync_desktop"] = "\uf41a";
        dict["SyncDesktop"] = "\uf41a";
        dict["sync_disabled"] = "\ue628";
        dict["SyncDisabled"] = "\ue628";
        dict["sync_lock"] = "\ueaee";
        dict["SyncLock"] = "\ueaee";
        dict["sync_problem"] = "\ue629";
        dict["SyncProblem"] = "\ue629";
        dict["sync_saved_locally"] = "\uf820";
        dict["SyncSavedLocally"] = "\uf820";
        dict["sync_saved_locally_off"] = "\uf264";
        dict["SyncSavedLocallyOff"] = "\uf264";
        dict["syringe"] = "\ue133";
        dict["system_security_update"] = "\uf2cd";
        dict["SystemSecurityUpdate"] = "\uf2cd";
        dict["system_security_update_good"] = "\uf073";
        dict["SystemSecurityUpdateGood"] = "\uf073";
        dict["system_security_update_warning"] = "\uf2d3";
        dict["SystemSecurityUpdateWarning"] = "\uf2d3";
        dict["system_update"] = "\uf2cd";
        dict["SystemUpdate"] = "\uf2cd";
        dict["system_update_alt"] = "\ue8d7";
        dict["SystemUpdateAlt"] = "\ue8d7";
        dict["tab"] = "\ue8d8";
        dict["tab_close"] = "\uf745";
        dict["TabClose"] = "\uf745";
        dict["tab_close_inactive"] = "\uf3d0";
        dict["TabCloseInactive"] = "\uf3d0";
        dict["tab_close_right"] = "\uf746";
        dict["TabCloseRight"] = "\uf746";
        dict["tab_duplicate"] = "\uf744";
        dict["TabDuplicate"] = "\uf744";
        dict["tab_group"] = "\uf743";
        dict["TabGroup"] = "\uf743";
        dict["tab_inactive"] = "\uf43b";
        dict["TabInactive"] = "\uf43b";
        dict["tab_move"] = "\uf742";
        dict["TabMove"] = "\uf742";
        dict["tab_new_right"] = "\uf741";
        dict["TabNewRight"] = "\uf741";
        dict["tab_recent"] = "\uf740";
        dict["TabRecent"] = "\uf740";
        dict["tab_search"] = "\uf2f2";
        dict["TabSearch"] = "\uf2f2";
        dict["tab_unselected"] = "\ue8d9";
        dict["TabUnselected"] = "\ue8d9";
        dict["table"] = "\uf191";
        dict["table_bar"] = "\uead2";
        dict["TableBar"] = "\uead2";
        dict["table_chart"] = "\ue265";
        dict["TableChart"] = "\ue265";
        dict["table_chart_view"] = "\uf6ef";
        dict["TableChartView"] = "\uf6ef";
        dict["table_convert"] = "\uf3c7";
        dict["TableConvert"] = "\uf3c7";
        dict["table_edit"] = "\uf3c6";
        dict["TableEdit"] = "\uf3c6";
        dict["table_eye"] = "\uf466";
        dict["TableEye"] = "\uf466";
        dict["table_lamp"] = "\ue1f2";
        dict["TableLamp"] = "\ue1f2";
        dict["table_large"] = "\uf299";
        dict["TableLarge"] = "\uf299";
        dict["table_restaurant"] = "\ueac6";
        dict["TableRestaurant"] = "\ueac6";
        dict["table_rows"] = "\uf101";
        dict["TableRows"] = "\uf101";
        dict["table_rows_narrow"] = "\uf73f";
        dict["TableRowsNarrow"] = "\uf73f";
        dict["table_sign"] = "\uef2c";
        dict["TableSign"] = "\uef2c";
        dict["table_view"] = "\uf1be";
        dict["TableView"] = "\uf1be";
        dict["tablet"] = "\ue32f";
        dict["tablet_android"] = "\ue330";
        dict["TabletAndroid"] = "\ue330";
        dict["tablet_camera"] = "\uf44d";
        dict["TabletCamera"] = "\uf44d";
        dict["tablet_mac"] = "\ue331";
        dict["TabletMac"] = "\ue331";
        dict["tabs"] = "\ue9ee";
        dict["tactic"] = "\uf564";
        dict["tag"] = "\ue9ef";
        dict["tag_faces"] = "\uea22";
        dict["TagFaces"] = "\uea22";
        dict["takeout_dining"] = "\uea74";
        dict["TakeoutDining"] = "\uea74";
        dict["takeout_dining_2"] = "\uef34";
        dict["TakeoutDining2"] = "\uef34";
        dict["tamper_detection_off"] = "\ue82e";
        dict["TamperDetectionOff"] = "\ue82e";
        dict["tamper_detection_on"] = "\uf8c8";
        dict["TamperDetectionOn"] = "\uf8c8";
        dict["tap_and_play"] = "\uf2cc";
        dict["TapAndPlay"] = "\uf2cc";
        dict["tapas"] = "\uf1e9";
        dict["target"] = "\ue719";
        dict["target_check"] = "\U000FFEB3";
        dict["TargetCheck"] = "\U000FFEB3";
        dict["task"] = "\uf075";
        dict["task_alt"] = "\ue2e6";
        dict["TaskAlt"] = "\ue2e6";
        dict["tatami_seat"] = "\uef33";
        dict["TatamiSeat"] = "\uef33";
        dict["taunt"] = "\uf69f";
        dict["taxi_alert"] = "\uef74";
        dict["TaxiAlert"] = "\uef74";
        dict["team_dashboard"] = "\ue013";
        dict["TeamDashboard"] = "\ue013";
        dict["temp_preferences_custom"] = "\uf8c9";
        dict["TempPreferencesCustom"] = "\uf8c9";
        dict["temp_preferences_eco"] = "\uf8ca";
        dict["TempPreferencesEco"] = "\uf8ca";
        dict["temple_buddhist"] = "\ueab3";
        dict["TempleBuddhist"] = "\ueab3";
        dict["temple_hindu"] = "\ueaaf";
        dict["TempleHindu"] = "\ueaaf";
        dict["tenancy"] = "\uf0e3";
        dict["terminal"] = "\ueb8e";
        dict["terminal_2"] = "\U000FFF8E";
        dict["Terminal2"] = "\U000FFF8E";
        dict["terminal_add"] = "\U000FFED3";
        dict["TerminalAdd"] = "\U000FFED3";
        dict["terrain"] = "\ue564";
        dict["text_ad"] = "\ue728";
        dict["TextAd"] = "\ue728";
        dict["text_ad_off"] = "\U000FFF92";
        dict["TextAdOff"] = "\U000FFF92";
        dict["text_compare"] = "\uf3c5";
        dict["TextCompare"] = "\uf3c5";
        dict["text_decrease"] = "\ueadd";
        dict["TextDecrease"] = "\ueadd";
        dict["text_fields"] = "\ue262";
        dict["TextFields"] = "\ue262";
        dict["text_fields_alt"] = "\ue9f1";
        dict["TextFieldsAlt"] = "\ue9f1";
        dict["text_format"] = "\ue165";
        dict["TextFormat"] = "\ue165";
        dict["text_increase"] = "\ueae2";
        dict["TextIncrease"] = "\ueae2";
        dict["text_rotate_up"] = "\ue93a";
        dict["TextRotateUp"] = "\ue93a";
        dict["text_rotate_vertical"] = "\ue93b";
        dict["TextRotateVertical"] = "\ue93b";
        dict["text_rotation_angledown"] = "\ue93c";
        dict["TextRotationAngledown"] = "\ue93c";
        dict["text_rotation_angleup"] = "\ue93d";
        dict["TextRotationAngleup"] = "\ue93d";
        dict["text_rotation_down"] = "\ue93e";
        dict["TextRotationDown"] = "\ue93e";
        dict["text_rotation_none"] = "\ue93f";
        dict["TextRotationNone"] = "\ue93f";
        dict["text_select_end"] = "\uf73e";
        dict["TextSelectEnd"] = "\uf73e";
        dict["text_select_jump_to_beginning"] = "\uf73d";
        dict["TextSelectJumpToBeginning"] = "\uf73d";
        dict["text_select_jump_to_end"] = "\uf73c";
        dict["TextSelectJumpToEnd"] = "\uf73c";
        dict["text_select_move_back_character"] = "\uf73b";
        dict["TextSelectMoveBackCharacter"] = "\uf73b";
        dict["text_select_move_back_word"] = "\uf73a";
        dict["TextSelectMoveBackWord"] = "\uf73a";
        dict["text_select_move_down"] = "\uf739";
        dict["TextSelectMoveDown"] = "\uf739";
        dict["text_select_move_forward_character"] = "\uf738";
        dict["TextSelectMoveForwardCharacter"] = "\uf738";
        dict["text_select_move_forward_word"] = "\uf737";
        dict["TextSelectMoveForwardWord"] = "\uf737";
        dict["text_select_move_up"] = "\uf736";
        dict["TextSelectMoveUp"] = "\uf736";
        dict["text_select_start"] = "\uf735";
        dict["TextSelectStart"] = "\uf735";
        dict["text_snippet"] = "\uf1c6";
        dict["TextSnippet"] = "\uf1c6";
        dict["text_to_speech"] = "\uf1bc";
        dict["TextToSpeech"] = "\uf1bc";
        dict["text_up"] = "\uf49e";
        dict["TextUp"] = "\uf49e";
        dict["textsms"] = "\ue625";
        dict["texture"] = "\ue421";
        dict["texture_add"] = "\uf57c";
        dict["TextureAdd"] = "\uf57c";
        dict["texture_minus"] = "\uf57b";
        dict["TextureMinus"] = "\uf57b";
        dict["theater_comedy"] = "\uea66";
        dict["TheaterComedy"] = "\uea66";
        dict["theaters"] = "\ue8da";
        dict["thermometer"] = "\ue846";
        dict["thermometer_add"] = "\uf582";
        dict["ThermometerAdd"] = "\uf582";
        dict["thermometer_alert"] = "\U000FFFFB";
        dict["ThermometerAlert"] = "\U000FFFFB";
        dict["thermometer_gain"] = "\uf6d8";
        dict["ThermometerGain"] = "\uf6d8";
        dict["thermometer_loss"] = "\uf6d7";
        dict["ThermometerLoss"] = "\uf6d7";
        dict["thermometer_minus"] = "\uf581";
        dict["ThermometerMinus"] = "\uf581";
        dict["thermostat"] = "\uf076";
        dict["thermostat_arrow_down"] = "\uf37a";
        dict["ThermostatArrowDown"] = "\uf37a";
        dict["thermostat_arrow_up"] = "\uf379";
        dict["ThermostatArrowUp"] = "\uf379";
        dict["thermostat_auto"] = "\uf077";
        dict["ThermostatAuto"] = "\uf077";
        dict["thermostat_carbon"] = "\uf178";
        dict["ThermostatCarbon"] = "\uf178";
        dict["things_to_do"] = "\ueb2a";
        dict["ThingsToDo"] = "\ueb2a";
        dict["thread_unread"] = "\uf4f9";
        dict["ThreadUnread"] = "\uf4f9";
        dict["threat_intelligence"] = "\ueaed";
        dict["ThreatIntelligence"] = "\ueaed";
        dict["thumb_down"] = "\uf578";
        dict["ThumbDown"] = "\uf578";
        dict["thumb_down_alt"] = "\uf578";
        dict["ThumbDownAlt"] = "\uf578";
        dict["thumb_down_filled"] = "\uf578";
        dict["ThumbDownFilled"] = "\uf578";
        dict["thumb_down_off"] = "\uf578";
        dict["ThumbDownOff"] = "\uf578";
        dict["thumb_down_off_alt"] = "\uf578";
        dict["ThumbDownOffAlt"] = "\uf578";
        dict["thumb_up"] = "\uf577";
        dict["ThumbUp"] = "\uf577";
        dict["thumb_up_alt"] = "\uf577";
        dict["ThumbUpAlt"] = "\uf577";
        dict["thumb_up_filled"] = "\uf577";
        dict["ThumbUpFilled"] = "\uf577";
        dict["thumb_up_off"] = "\uf577";
        dict["ThumbUpOff"] = "\uf577";
        dict["thumb_up_off_alt"] = "\uf577";
        dict["ThumbUpOffAlt"] = "\uf577";
        dict["thumbnail_bar"] = "\uf734";
        dict["ThumbnailBar"] = "\uf734";
        dict["thumbs_up_double"] = "\ueefc";
        dict["ThumbsUpDouble"] = "\ueefc";
        dict["thumbs_up_down"] = "\ue8dd";
        dict["ThumbsUpDown"] = "\ue8dd";
        dict["thunderstorm"] = "\uebdb";
        dict["tibia"] = "\uf89b";
        dict["tibia_alt"] = "\uf89c";
        dict["TibiaAlt"] = "\uf89c";
        dict["tile_large"] = "\uf3c3";
        dict["TileLarge"] = "\uf3c3";
        dict["tile_medium"] = "\uf3c2";
        dict["TileMedium"] = "\uf3c2";
        dict["tile_small"] = "\uf3c1";
        dict["TileSmall"] = "\uf3c1";
        dict["tilt_arrow_down"] = "\U000FFF26";
        dict["TiltArrowDown"] = "\U000FFF26";
        dict["tilt_arrow_up"] = "\U000FFF25";
        dict["TiltArrowUp"] = "\U000FFF25";
        dict["time_auto"] = "\uf0e4";
        dict["TimeAuto"] = "\uf0e4";
        dict["time_to_leave"] = "\ueff7";
        dict["TimeToLeave"] = "\ueff7";
        dict["timelapse"] = "\ue422";
        dict["timeline"] = "\ue922";
        dict["timer"] = "\ue425";
        dict["timer_1"] = "\uf2af";
        dict["Timer1"] = "\uf2af";
        dict["timer_10"] = "\ue423";
        dict["Timer10"] = "\ue423";
        dict["timer_10_alt_1"] = "\uefbf";
        dict["Timer10Alt1"] = "\uefbf";
        dict["timer_10_select"] = "\uf07a";
        dict["Timer10Select"] = "\uf07a";
        dict["timer_2"] = "\uf2ae";
        dict["Timer2"] = "\uf2ae";
        dict["timer_3"] = "\ue424";
        dict["Timer3"] = "\ue424";
        dict["timer_3_alt_1"] = "\uefc0";
        dict["Timer3Alt1"] = "\uefc0";
        dict["timer_3_select"] = "\uf07b";
        dict["Timer3Select"] = "\uf07b";
        dict["timer_5"] = "\uf4b1";
        dict["Timer5"] = "\uf4b1";
        dict["timer_5_shutter"] = "\uf4b2";
        dict["Timer5Shutter"] = "\uf4b2";
        dict["timer_arrow_down"] = "\uf378";
        dict["TimerArrowDown"] = "\uf378";
        dict["timer_arrow_up"] = "\uf377";
        dict["TimerArrowUp"] = "\uf377";
        dict["timer_off"] = "\ue426";
        dict["TimerOff"] = "\ue426";
        dict["timer_pause"] = "\uf4bb";
        dict["TimerPause"] = "\uf4bb";
        dict["timer_play"] = "\uf4ba";
        dict["TimerPlay"] = "\uf4ba";
        dict["tips_and_updates"] = "\ue79a";
        dict["TipsAndUpdates"] = "\ue79a";
        dict["tire_repair"] = "\uebc8";
        dict["TireRepair"] = "\uebc8";
        dict["title"] = "\ue264";
        dict["titlecase"] = "\uf489";
        dict["toast"] = "\uefc1";
        dict["toc"] = "\ue8de";
        dict["today"] = "\ue8df";
        dict["toggle_off"] = "\ue9f5";
        dict["ToggleOff"] = "\ue9f5";
        dict["toggle_on"] = "\ue9f6";
        dict["ToggleOn"] = "\ue9f6";
        dict["token"] = "\uea25";
        dict["toll"] = "\ue8e0";
        dict["tonality"] = "\ue427";
        dict["tonality_2"] = "\uf2b4";
        dict["Tonality2"] = "\uf2b4";
        dict["toolbar"] = "\ue9f7";
        dict["tools_flat_head"] = "\uf8cb";
        dict["ToolsFlatHead"] = "\uf8cb";
        dict["tools_installation_kit"] = "\ue2ab";
        dict["ToolsInstallationKit"] = "\ue2ab";
        dict["tools_ladder"] = "\ue2cb";
        dict["ToolsLadder"] = "\ue2cb";
        dict["tools_level"] = "\ue77b";
        dict["ToolsLevel"] = "\ue77b";
        dict["tools_phillips"] = "\uf8cc";
        dict["ToolsPhillips"] = "\uf8cc";
        dict["tools_pliers_wire_stripper"] = "\ue2aa";
        dict["ToolsPliersWireStripper"] = "\ue2aa";
        dict["tools_power_drill"] = "\ue1e9";
        dict["ToolsPowerDrill"] = "\ue1e9";
        dict["tools_wrench"] = "\uf8cd";
        dict["ToolsWrench"] = "\uf8cd";
        dict["tooltip"] = "\ue9f8";
        dict["tooltip_2"] = "\uf3ed";
        dict["Tooltip2"] = "\uf3ed";
        dict["top_panel_close"] = "\uf733";
        dict["TopPanelClose"] = "\uf733";
        dict["top_panel_open"] = "\uf732";
        dict["TopPanelOpen"] = "\uf732";
        dict["topic"] = "\uf1c8";
        dict["tornado"] = "\ue199";
        dict["total_dissolved_solids"] = "\uf877";
        dict["TotalDissolvedSolids"] = "\uf877";
        dict["touch_app"] = "\ue913";
        dict["TouchApp"] = "\ue913";
        dict["touch_double"] = "\uf38b";
        dict["TouchDouble"] = "\uf38b";
        dict["touch_double_2"] = "\U000FFF35";
        dict["TouchDouble2"] = "\U000FFF35";
        dict["touch_long"] = "\uf38a";
        dict["TouchLong"] = "\uf38a";
        dict["touch_triple"] = "\uf389";
        dict["TouchTriple"] = "\uf389";
        dict["touchpad_mouse"] = "\uf687";
        dict["TouchpadMouse"] = "\uf687";
        dict["touchpad_mouse_off"] = "\uf4e6";
        dict["TouchpadMouseOff"] = "\uf4e6";
        dict["tour"] = "\uef75";
        dict["toys"] = "\ue332";
        dict["toys_and_games"] = "\uefc2";
        dict["ToysAndGames"] = "\uefc2";
        dict["toys_fan"] = "\uf887";
        dict["ToysFan"] = "\uf887";
        dict["track_changes"] = "\ue8e1";
        dict["TrackChanges"] = "\ue8e1";
        dict["trackpad_input"] = "\uf4c7";
        dict["TrackpadInput"] = "\uf4c7";
        dict["trackpad_input_2"] = "\uf409";
        dict["TrackpadInput2"] = "\uf409";
        dict["trackpad_input_3"] = "\uf408";
        dict["TrackpadInput3"] = "\uf408";
        dict["traffic"] = "\ue565";
        dict["traffic_jam"] = "\uf46f";
        dict["TrafficJam"] = "\uf46f";
        dict["trail_length"] = "\ueb5e";
        dict["TrailLength"] = "\ueb5e";
        dict["trail_length_medium"] = "\ueb63";
        dict["TrailLengthMedium"] = "\ueb63";
        dict["trail_length_short"] = "\ueb6d";
        dict["TrailLengthShort"] = "\ueb6d";
        dict["train"] = "\ue570";
        dict["tram"] = "\ue571";
        dict["transcribe"] = "\uf8ec";
        dict["transfer_within_a_station"] = "\ue572";
        dict["TransferWithinAStation"] = "\ue572";
        dict["transform"] = "\ue428";
        dict["transgender"] = "\ue58d";
        dict["transit_enterexit"] = "\ue579";
        dict["TransitEnterexit"] = "\ue579";
        dict["transit_ticket"] = "\uf3f1";
        dict["TransitTicket"] = "\uf3f1";
        dict["transition_chop"] = "\uf50e";
        dict["TransitionChop"] = "\uf50e";
        dict["transition_dissolve"] = "\uf50d";
        dict["TransitionDissolve"] = "\uf50d";
        dict["transition_fade"] = "\uf50c";
        dict["TransitionFade"] = "\uf50c";
        dict["transition_push"] = "\uf50b";
        dict["TransitionPush"] = "\uf50b";
        dict["transition_slide"] = "\uf50a";
        dict["TransitionSlide"] = "\uf50a";
        dict["translate"] = "\ue8e2";
        dict["translate_indic"] = "\uf263";
        dict["TranslateIndic"] = "\uf263";
        dict["transportation"] = "\ue21d";
        dict["travel"] = "\uef93";
        dict["travel_explore"] = "\ue2db";
        dict["TravelExplore"] = "\ue2db";
        dict["travel_luggage_and_bags"] = "\uefc3";
        dict["TravelLuggageAndBags"] = "\uefc3";
        dict["trending_down"] = "\ue8e3";
        dict["TrendingDown"] = "\ue8e3";
        dict["trending_flat"] = "\ue8e4";
        dict["TrendingFlat"] = "\ue8e4";
        dict["trending_up"] = "\ue8e5";
        dict["TrendingUp"] = "\ue8e5";
        dict["triangle_circle"] = "\ueec6";
        dict["TriangleCircle"] = "\ueec6";
        dict["trip"] = "\ue6fb";
        dict["trip_origin"] = "\ue57b";
        dict["TripOrigin"] = "\ue57b";
        dict["trolley"] = "\uf86b";
        dict["trolley_cable_car"] = "\uf46e";
        dict["TrolleyCableCar"] = "\uf46e";
        dict["trophy"] = "\uea23";
        dict["troubleshoot"] = "\ue1d2";
        dict["try"] = "\uf07c";
        dict["tsunami"] = "\uebd8";
        dict["tsv"] = "\ue6d6";
        dict["tty"] = "\uf1aa";
        dict["tune"] = "\ue429";
        dict["tungsten"] = "\uf07d";
        dict["turn_left"] = "\ueba6";
        dict["TurnLeft"] = "\ueba6";
        dict["turn_right"] = "\uebab";
        dict["TurnRight"] = "\uebab";
        dict["turn_sharp_left"] = "\ueba7";
        dict["TurnSharpLeft"] = "\ueba7";
        dict["turn_sharp_right"] = "\uebaa";
        dict["TurnSharpRight"] = "\uebaa";
        dict["turn_slight_left"] = "\ueba4";
        dict["TurnSlightLeft"] = "\ueba4";
        dict["turn_slight_right"] = "\ueb9a";
        dict["TurnSlightRight"] = "\ueb9a";
        dict["turned_in"] = "\ue8e7";
        dict["TurnedIn"] = "\ue8e7";
        dict["turned_in_not"] = "\ue8e7";
        dict["TurnedInNot"] = "\ue8e7";
        dict["tv"] = "\ue63b";
        dict["tv_displays"] = "\uf3ec";
        dict["TvDisplays"] = "\uf3ec";
        dict["tv_gen"] = "\ue830";
        dict["TvGen"] = "\ue830";
        dict["tv_guide"] = "\ue1dc";
        dict["TvGuide"] = "\ue1dc";
        dict["tv_next"] = "\uf3eb";
        dict["TvNext"] = "\uf3eb";
        dict["tv_off"] = "\ue647";
        dict["TvOff"] = "\ue647";
        dict["tv_options_edit_channels"] = "\ue1dd";
        dict["TvOptionsEditChannels"] = "\ue1dd";
        dict["tv_options_input_settings"] = "\ue1de";
        dict["TvOptionsInputSettings"] = "\ue1de";
        dict["tv_remote"] = "\uf5d9";
        dict["TvRemote"] = "\uf5d9";
        dict["tv_signin"] = "\ue71b";
        dict["TvSignin"] = "\ue71b";
        dict["tv_with_assistant"] = "\ue785";
        dict["TvWithAssistant"] = "\ue785";
        dict["two_pager"] = "\uf51f";
        dict["TwoPager"] = "\uf51f";
        dict["two_pager_store"] = "\uf3c4";
        dict["TwoPagerStore"] = "\uf3c4";
        dict["two_wheeler"] = "\ue9f9";
        dict["TwoWheeler"] = "\ue9f9";
        dict["type_specimen"] = "\uf8f0";
        dict["TypeSpecimen"] = "\uf8f0";
        dict["u_turn_left"] = "\ueba1";
        dict["UTurnLeft"] = "\ueba1";
        dict["u_turn_right"] = "\ueba2";
        dict["UTurnRight"] = "\ueba2";
        dict["udon"] = "\uef32";
        dict["ulna_radius"] = "\uf89d";
        dict["UlnaRadius"] = "\uf89d";
        dict["ulna_radius_alt"] = "\uf89e";
        dict["UlnaRadiusAlt"] = "\uf89e";
        dict["umbrella"] = "\uf1ad";
        dict["unarchive"] = "\ue169";
        dict["undereye"] = "\ueeb1";
        dict["undo"] = "\ue166";
        dict["unfold_less"] = "\ue5d6";
        dict["UnfoldLess"] = "\ue5d6";
        dict["unfold_less_double"] = "\uf8cf";
        dict["UnfoldLessDouble"] = "\uf8cf";
        dict["unfold_more"] = "\ue5d7";
        dict["UnfoldMore"] = "\ue5d7";
        dict["unfold_more_double"] = "\uf8d0";
        dict["UnfoldMoreDouble"] = "\uf8d0";
        dict["ungroup"] = "\uf731";
        dict["universal_currency"] = "\ue9fa";
        dict["UniversalCurrency"] = "\ue9fa";
        dict["universal_currency_alt"] = "\ue734";
        dict["UniversalCurrencyAlt"] = "\ue734";
        dict["universal_local"] = "\ue9fb";
        dict["UniversalLocal"] = "\ue9fb";
        dict["unknown_2"] = "\uf49f";
        dict["Unknown2"] = "\uf49f";
        dict["unknown_5"] = "\ue6a5";
        dict["Unknown5"] = "\ue6a5";
        dict["unknown_7"] = "\uf49e";
        dict["Unknown7"] = "\uf49e";
        dict["unknown_document"] = "\uf804";
        dict["UnknownDocument"] = "\uf804";
        dict["unknown_med"] = "\ueabd";
        dict["UnknownMed"] = "\ueabd";
        dict["unlicense"] = "\ueb05";
        dict["unpaved_road"] = "\uf46d";
        dict["UnpavedRoad"] = "\uf46d";
        dict["unpin"] = "\ue6f9";
        dict["unpublished"] = "\uf236";
        dict["unsubscribe"] = "\ue0eb";
        dict["upcoming"] = "\uf07e";
        dict["update"] = "\ue923";
        dict["update_disabled"] = "\ue075";
        dict["UpdateDisabled"] = "\ue075";
        dict["upgrade"] = "\uf0fb";
        dict["upi_pay"] = "\uf3cf";
        dict["UpiPay"] = "\uf3cf";
        dict["upload"] = "\uf09b";
        dict["upload_2"] = "\uf521";
        dict["Upload2"] = "\uf521";
        dict["upload_file"] = "\ue9fc";
        dict["UploadFile"] = "\ue9fc";
        dict["uppercase"] = "\uf488";
        dict["urology"] = "\ue137";
        dict["usb"] = "\ue1e0";
        dict["usb_off"] = "\ue4fa";
        dict["UsbOff"] = "\ue4fa";
        dict["user_attributes"] = "\ue708";
        dict["UserAttributes"] = "\ue708";
        dict["vaccines"] = "\ue138";
        dict["vacuum"] = "\uefc5";
        dict["vacuum_2"] = "\U000FFF6D";
        dict["Vacuum2"] = "\U000FFF6D";
        dict["vacuum_2_on"] = "\U000FFF6E";
        dict["Vacuum2On"] = "\U000FFF6E";
        dict["valve"] = "\ue224";
        dict["vape_free"] = "\uebc6";
        dict["VapeFree"] = "\uebc6";
        dict["vaping_rooms"] = "\uebcf";
        dict["VapingRooms"] = "\uebcf";
        dict["variable_add"] = "\uf51e";
        dict["VariableAdd"] = "\uf51e";
        dict["variable_insert"] = "\uf51d";
        dict["VariableInsert"] = "\uf51d";
        dict["variable_remove"] = "\uf51c";
        dict["VariableRemove"] = "\uf51c";
        dict["variables"] = "\uf851";
        dict["ventilator"] = "\ue139";
        dict["verified"] = "\uef76";
        dict["verified_off"] = "\uf30e";
        dict["VerifiedOff"] = "\uf30e";
        dict["verified_user"] = "\uf013";
        dict["VerifiedUser"] = "\uf013";
        dict["vertical_align_bottom"] = "\ue258";
        dict["VerticalAlignBottom"] = "\ue258";
        dict["vertical_align_center"] = "\ue259";
        dict["VerticalAlignCenter"] = "\ue259";
        dict["vertical_align_top"] = "\ue25a";
        dict["VerticalAlignTop"] = "\ue25a";
        dict["vertical_distribute"] = "\ue076";
        dict["VerticalDistribute"] = "\ue076";
        dict["vertical_shades"] = "\uec0e";
        dict["VerticalShades"] = "\uec0e";
        dict["vertical_shades_closed"] = "\uec0d";
        dict["VerticalShadesClosed"] = "\uec0d";
        dict["vertical_split"] = "\ue949";
        dict["VerticalSplit"] = "\ue949";
        dict["vibration"] = "\uf2cb";
        dict["video_call"] = "\ue070";
        dict["VideoCall"] = "\ue070";
        dict["video_camera_back"] = "\uf07f";
        dict["VideoCameraBack"] = "\uf07f";
        dict["video_camera_back_add"] = "\uf40c";
        dict["VideoCameraBackAdd"] = "\uf40c";
        dict["video_camera_front"] = "\uf080";
        dict["VideoCameraFront"] = "\uf080";
        dict["video_camera_front_off"] = "\uf83b";
        dict["VideoCameraFrontOff"] = "\uf83b";
        dict["video_chat"] = "\uf8a0";
        dict["VideoChat"] = "\uf8a0";
        dict["video_file"] = "\ueb87";
        dict["VideoFile"] = "\ueb87";
        dict["video_frame_copy"] = "\U000FFF0D";
        dict["VideoFrameCopy"] = "\U000FFF0D";
        dict["video_frame_save"] = "\U000FFF0C";
        dict["VideoFrameSave"] = "\U000FFF0C";
        dict["video_label"] = "\ue071";
        dict["VideoLabel"] = "\ue071";
        dict["video_library"] = "\ue04a";
        dict["VideoLibrary"] = "\ue04a";
        dict["video_search"] = "\uefc6";
        dict["VideoSearch"] = "\uefc6";
        dict["video_settings"] = "\uea75";
        dict["VideoSettings"] = "\uea75";
        dict["video_stable"] = "\uf081";
        dict["VideoStable"] = "\uf081";
        dict["video_template"] = "\U000FFFD3";
        dict["VideoTemplate"] = "\U000FFFD3";
        dict["videocam"] = "\ue04b";
        dict["videocam_alert"] = "\uf390";
        dict["VideocamAlert"] = "\uf390";
        dict["videocam_off"] = "\ue04c";
        dict["VideocamOff"] = "\ue04c";
        dict["videogame_asset"] = "\ue338";
        dict["VideogameAsset"] = "\ue338";
        dict["videogame_asset_off"] = "\ue500";
        dict["VideogameAssetOff"] = "\ue500";
        dict["view_agenda"] = "\ue8e9";
        dict["ViewAgenda"] = "\ue8e9";
        dict["view_apps"] = "\uf376";
        dict["ViewApps"] = "\uf376";
        dict["view_array"] = "\ue8ea";
        dict["ViewArray"] = "\ue8ea";
        dict["view_carousel"] = "\ue8eb";
        dict["ViewCarousel"] = "\ue8eb";
        dict["view_column"] = "\ue8ec";
        dict["ViewColumn"] = "\ue8ec";
        dict["view_column_2"] = "\uf847";
        dict["ViewColumn2"] = "\uf847";
        dict["view_comfy"] = "\ue42a";
        dict["ViewComfy"] = "\ue42a";
        dict["view_comfy_alt"] = "\ueb73";
        dict["ViewComfyAlt"] = "\ueb73";
        dict["view_compact"] = "\ue42b";
        dict["ViewCompact"] = "\ue42b";
        dict["view_compact_alt"] = "\ueb74";
        dict["ViewCompactAlt"] = "\ueb74";
        dict["view_cozy"] = "\ueb75";
        dict["ViewCozy"] = "\ueb75";
        dict["view_day"] = "\ue8ed";
        dict["ViewDay"] = "\ue8ed";
        dict["view_headline"] = "\ue8ee";
        dict["ViewHeadline"] = "\ue8ee";
        dict["view_in_ar"] = "\uefc9";
        dict["ViewInAr"] = "\uefc9";
        dict["view_in_ar_new"] = "\uefc9";
        dict["ViewInArNew"] = "\uefc9";
        dict["view_in_ar_off"] = "\uf61b";
        dict["ViewInArOff"] = "\uf61b";
        dict["view_kanban"] = "\ueb7f";
        dict["ViewKanban"] = "\ueb7f";
        dict["view_list"] = "\ue8ef";
        dict["ViewList"] = "\ue8ef";
        dict["view_module"] = "\ue8f0";
        dict["ViewModule"] = "\ue8f0";
        dict["view_object_track"] = "\uf432";
        dict["ViewObjectTrack"] = "\uf432";
        dict["view_quilt"] = "\ue8f1";
        dict["ViewQuilt"] = "\ue8f1";
        dict["view_real_size"] = "\uf4c2";
        dict["ViewRealSize"] = "\uf4c2";
        dict["view_sidebar"] = "\uf114";
        dict["ViewSidebar"] = "\uf114";
        dict["view_stream"] = "\ue8f2";
        dict["ViewStream"] = "\ue8f2";
        dict["view_timeline"] = "\ueb85";
        dict["ViewTimeline"] = "\ueb85";
        dict["view_week"] = "\ue8f3";
        dict["ViewWeek"] = "\ue8f3";
        dict["vignette"] = "\ue435";
        dict["vignette_2"] = "\uf2b3";
        dict["Vignette2"] = "\uf2b3";
        dict["villa"] = "\ue586";
        dict["visibility"] = "\ue8f4";
        dict["visibility_lock"] = "\uf653";
        dict["VisibilityLock"] = "\uf653";
        dict["visibility_off"] = "\ue8f5";
        dict["VisibilityOff"] = "\ue8f5";
        dict["vital_signs"] = "\ue650";
        dict["VitalSigns"] = "\ue650";
        dict["vitals"] = "\ue13b";
        dict["vo2_max"] = "\uf4aa";
        dict["Vo2Max"] = "\uf4aa";
        dict["voice_chat"] = "\ue62e";
        dict["VoiceChat"] = "\ue62e";
        dict["voice_chat_off"] = "\ueebe";
        dict["VoiceChatOff"] = "\ueebe";
        dict["voice_over_off"] = "\ue94a";
        dict["VoiceOverOff"] = "\ue94a";
        dict["voice_selection"] = "\uf58a";
        dict["VoiceSelection"] = "\uf58a";
        dict["voice_selection_off"] = "\uf42c";
        dict["VoiceSelectionOff"] = "\uf42c";
        dict["voicemail"] = "\ue0d9";
        dict["voicemail_2"] = "\uf352";
        dict["Voicemail2"] = "\uf352";
        dict["volcano"] = "\uebda";
        dict["volume_down"] = "\ue04d";
        dict["VolumeDown"] = "\ue04d";
        dict["volume_down_alt"] = "\ue79c";
        dict["VolumeDownAlt"] = "\ue79c";
        dict["volume_mute"] = "\ue04e";
        dict["VolumeMute"] = "\ue04e";
        dict["volume_off"] = "\ue04f";
        dict["VolumeOff"] = "\ue04f";
        dict["volume_up"] = "\ue050";
        dict["VolumeUp"] = "\ue050";
        dict["volunteer_activism"] = "\uea70";
        dict["VolunteerActivism"] = "\uea70";
        dict["voting_chip"] = "\uf852";
        dict["VotingChip"] = "\uf852";
        dict["vpn_key"] = "\ue0da";
        dict["VpnKey"] = "\ue0da";
        dict["vpn_key_alert"] = "\uf6cc";
        dict["VpnKeyAlert"] = "\uf6cc";
        dict["vpn_key_off"] = "\ueb7a";
        dict["VpnKeyOff"] = "\ueb7a";
        dict["vpn_lock"] = "\ue62f";
        dict["VpnLock"] = "\ue62f";
        dict["vpn_lock_2"] = "\uf350";
        dict["VpnLock2"] = "\uf350";
        dict["vr180_create2d"] = "\uefca";
        dict["Vr180Create2d"] = "\uefca";
        dict["vr180_create2d_off"] = "\uf571";
        dict["Vr180Create2dOff"] = "\uf571";
        dict["vrpano"] = "\uf082";
        dict["walk_bike"] = "\U000FFF01";
        dict["WalkBike"] = "\U000FFF01";
        dict["wall_art"] = "\uefcb";
        dict["WallArt"] = "\uefcb";
        dict["wall_lamp"] = "\ue2b4";
        dict["WallLamp"] = "\ue2b4";
        dict["wallet"] = "\uf8ff";
        dict["wallpaper"] = "\ue1bc";
        dict["wallpaper_slideshow"] = "\uf672";
        dict["WallpaperSlideshow"] = "\uf672";
        dict["wand_shine"] = "\uf31f";
        dict["WandShine"] = "\uf31f";
        dict["wand_stars"] = "\uf31e";
        dict["WandStars"] = "\uf31e";
        dict["ward"] = "\ue13c";
        dict["warehouse"] = "\uebb8";
        dict["warning"] = "\uf083";
        dict["warning_amber"] = "\uf083";
        dict["WarningAmber"] = "\uf083";
        dict["warning_off"] = "\uf7ad";
        dict["WarningOff"] = "\uf7ad";
        dict["wash"] = "\uf1b1";
        dict["washoku"] = "\uf280";
        dict["watch"] = "\ue334";
        dict["watch_alert"] = "\U000FFFD1";
        dict["WatchAlert"] = "\U000FFFD1";
        dict["watch_arrow"] = "\uf2ca";
        dict["WatchArrow"] = "\uf2ca";
        dict["watch_arrow_down"] = "\U000FFFCF";
        dict["WatchArrowDown"] = "\U000FFFCF";
        dict["watch_button"] = "\U000FFF20";
        dict["WatchButton"] = "\U000FFF20";
        dict["watch_button_press"] = "\uf6aa";
        dict["WatchButtonPress"] = "\uf6aa";
        dict["watch_check"] = "\uf468";
        dict["WatchCheck"] = "\uf468";
        dict["watch_later"] = "\uefd6";
        dict["WatchLater"] = "\uefd6";
        dict["watch_lock"] = "\ueee9";
        dict["WatchLock"] = "\ueee9";
        dict["watch_off"] = "\ueae3";
        dict["WatchOff"] = "\ueae3";
        dict["watch_screentime"] = "\uf6ae";
        dict["WatchScreentime"] = "\uf6ae";
        dict["watch_vibration"] = "\uf467";
        dict["WatchVibration"] = "\uf467";
        dict["watch_wake"] = "\uf6a9";
        dict["WatchWake"] = "\uf6a9";
        dict["water"] = "\uf084";
        dict["water_bottle"] = "\uf69d";
        dict["WaterBottle"] = "\uf69d";
        dict["water_bottle_large"] = "\uf69e";
        dict["WaterBottleLarge"] = "\uf69e";
        dict["water_damage"] = "\uf203";
        dict["WaterDamage"] = "\uf203";
        dict["water_do"] = "\uf870";
        dict["WaterDo"] = "\uf870";
        dict["water_drop"] = "\ue798";
        dict["WaterDrop"] = "\ue798";
        dict["water_drops"] = "\U000FFFA5";
        dict["WaterDrops"] = "\U000FFFA5";
        dict["water_ec"] = "\uf875";
        dict["WaterEc"] = "\uf875";
        dict["water_full"] = "\uf6d6";
        dict["WaterFull"] = "\uf6d6";
        dict["water_heater"] = "\ue284";
        dict["WaterHeater"] = "\ue284";
        dict["water_lock"] = "\uf6ad";
        dict["WaterLock"] = "\uf6ad";
        dict["water_loss"] = "\uf6d5";
        dict["WaterLoss"] = "\uf6d5";
        dict["water_lux"] = "\uf874";
        dict["WaterLux"] = "\uf874";
        dict["water_medium"] = "\uf6d4";
        dict["WaterMedium"] = "\uf6d4";
        dict["water_orp"] = "\uf878";
        dict["WaterOrp"] = "\uf878";
        dict["water_ph"] = "\uf87a";
        dict["WaterPh"] = "\uf87a";
        dict["water_pump"] = "\uf5d8";
        dict["WaterPump"] = "\uf5d8";
        dict["water_voc"] = "\uf87b";
        dict["WaterVoc"] = "\uf87b";
        dict["waterfall_chart"] = "\uea00";
        dict["WaterfallChart"] = "\uea00";
        dict["waves"] = "\ue176";
        dict["waving_hand"] = "\ue766";
        dict["WavingHand"] = "\ue766";
        dict["wb_auto"] = "\ue42c";
        dict["WbAuto"] = "\ue42c";
        dict["wb_cloudy"] = "\uf15c";
        dict["WbCloudy"] = "\uf15c";
        dict["wb_incandescent"] = "\ue42e";
        dict["WbIncandescent"] = "\ue42e";
        dict["wb_iridescent"] = "\uf07d";
        dict["WbIridescent"] = "\uf07d";
        dict["wb_shade"] = "\uea01";
        dict["WbShade"] = "\uea01";
        dict["wb_sunny"] = "\ue430";
        dict["WbSunny"] = "\ue430";
        dict["wb_twilight"] = "\ue1c6";
        dict["WbTwilight"] = "\ue1c6";
        dict["wb_twilight_2"] = "\U000FFF1F";
        dict["WbTwilight2"] = "\U000FFF1F";
        dict["wc"] = "\ue63d";
        dict["weather_hail"] = "\uf67f";
        dict["WeatherHail"] = "\uf67f";
        dict["weather_mix"] = "\uf60b";
        dict["WeatherMix"] = "\uf60b";
        dict["weather_snowy"] = "\ue2cd";
        dict["WeatherSnowy"] = "\ue2cd";
        dict["web"] = "\ue66a";
        dict["web_asset"] = "\ue069";
        dict["WebAsset"] = "\ue069";
        dict["web_asset_off"] = "\uef47";
        dict["WebAssetOff"] = "\uef47";
        dict["web_stories"] = "\ue595";
        dict["WebStories"] = "\ue595";
        dict["web_traffic"] = "\uea03";
        dict["WebTraffic"] = "\uea03";
        dict["webhook"] = "\ueb92";
        dict["weekend"] = "\ue16b";
        dict["weight"] = "\ue13d";
        dict["west"] = "\uf1e6";
        dict["whatshot"] = "\ue80e";
        dict["wheat"] = "\U000FFFA4";
        dict["wheelchair_pickup"] = "\uf1ab";
        dict["WheelchairPickup"] = "\uf1ab";
        dict["where_to_vote"] = "\ue177";
        dict["WhereToVote"] = "\ue177";
        dict["widget_medium"] = "\uf3ba";
        dict["WidgetMedium"] = "\uf3ba";
        dict["widget_menu"] = "\ueeb7";
        dict["WidgetMenu"] = "\ueeb7";
        dict["widget_small"] = "\uf3b9";
        dict["WidgetSmall"] = "\uf3b9";
        dict["widget_width"] = "\uf3b8";
        dict["WidgetWidth"] = "\uf3b8";
        dict["widgets"] = "\ue1bd";
        dict["width"] = "\uf730";
        dict["width_full"] = "\uf8f5";
        dict["WidthFull"] = "\uf8f5";
        dict["width_normal"] = "\uf8f6";
        dict["WidthNormal"] = "\uf8f6";
        dict["width_wide"] = "\uf8f7";
        dict["WidthWide"] = "\uf8f7";
        dict["wifi"] = "\ue63e";
        dict["wifi_1_bar"] = "\ue4ca";
        dict["Wifi1Bar"] = "\ue4ca";
        dict["wifi_2_bar"] = "\ue4d9";
        dict["Wifi2Bar"] = "\ue4d9";
        dict["wifi_add"] = "\uf7a8";
        dict["WifiAdd"] = "\uf7a8";
        dict["wifi_calling"] = "\uef77";
        dict["WifiCalling"] = "\uef77";
        dict["wifi_calling_1"] = "\uf0e7";
        dict["WifiCalling1"] = "\uf0e7";
        dict["wifi_calling_2"] = "\uf0f6";
        dict["WifiCalling2"] = "\uf0f6";
        dict["wifi_calling_3"] = "\uf0e7";
        dict["WifiCalling3"] = "\uf0e7";
        dict["wifi_calling_bar_1"] = "\uf44c";
        dict["WifiCallingBar1"] = "\uf44c";
        dict["wifi_calling_bar_2"] = "\uf44b";
        dict["WifiCallingBar2"] = "\uf44b";
        dict["wifi_calling_bar_3"] = "\uf44a";
        dict["WifiCallingBar3"] = "\uf44a";
        dict["wifi_channel"] = "\ueb6a";
        dict["WifiChannel"] = "\ueb6a";
        dict["wifi_device"] = "\U000FFF34";
        dict["WifiDevice"] = "\U000FFF34";
        dict["wifi_find"] = "\ueb31";
        dict["WifiFind"] = "\ueb31";
        dict["wifi_home"] = "\uf671";
        dict["WifiHome"] = "\uf671";
        dict["wifi_lock"] = "\ue1e1";
        dict["WifiLock"] = "\ue1e1";
        dict["wifi_notification"] = "\uf670";
        dict["WifiNotification"] = "\uf670";
        dict["wifi_off"] = "\ue648";
        dict["WifiOff"] = "\ue648";
        dict["wifi_password"] = "\ueb6b";
        dict["WifiPassword"] = "\ueb6b";
        dict["wifi_protected_setup"] = "\uf0fc";
        dict["WifiProtectedSetup"] = "\uf0fc";
        dict["wifi_proxy"] = "\uf7a7";
        dict["WifiProxy"] = "\uf7a7";
        dict["wifi_tethering"] = "\ue1e2";
        dict["WifiTethering"] = "\ue1e2";
        dict["wifi_tethering_error"] = "\uead9";
        dict["WifiTetheringError"] = "\uead9";
        dict["wifi_tethering_off"] = "\uf087";
        dict["WifiTetheringOff"] = "\uf087";
        dict["wind_power"] = "\uec0c";
        dict["WindPower"] = "\uec0c";
        dict["window"] = "\uf088";
        dict["window_closed"] = "\ue77e";
        dict["WindowClosed"] = "\ue77e";
        dict["window_open"] = "\ue78c";
        dict["WindowOpen"] = "\ue78c";
        dict["window_sensor"] = "\ue2bb";
        dict["WindowSensor"] = "\ue2bb";
        dict["windshield_defrost_auto"] = "\uf248";
        dict["WindshieldDefrostAuto"] = "\uf248";
        dict["windshield_defrost_front"] = "\uf32a";
        dict["WindshieldDefrostFront"] = "\uf32a";
        dict["windshield_defrost_rear"] = "\uf329";
        dict["WindshieldDefrostRear"] = "\uf329";
        dict["windshield_heat_front"] = "\uf328";
        dict["WindshieldHeatFront"] = "\uf328";
        dict["wine_bar"] = "\uf1e8";
        dict["WineBar"] = "\uf1e8";
        dict["woman"] = "\ue13e";
        dict["woman_2"] = "\uf8e7";
        dict["Woman2"] = "\uf8e7";
        dict["work"] = "\ue943";
        dict["work_alert"] = "\uf5f7";
        dict["WorkAlert"] = "\uf5f7";
        dict["work_history"] = "\uec09";
        dict["WorkHistory"] = "\uec09";
        dict["work_off"] = "\ue942";
        dict["WorkOff"] = "\ue942";
        dict["work_outline"] = "\ue943";
        dict["WorkOutline"] = "\ue943";
        dict["work_update"] = "\uf5f8";
        dict["WorkUpdate"] = "\uf5f8";
        dict["workflow"] = "\uea04";
        dict["workspace_premium"] = "\ue7af";
        dict["WorkspacePremium"] = "\ue7af";
        dict["workspaces"] = "\uea0f";
        dict["workspaces_outline"] = "\uea0f";
        dict["WorkspacesOutline"] = "\uea0f";
        dict["wounds_injuries"] = "\ue13f";
        dict["WoundsInjuries"] = "\ue13f";
        dict["wrap_text"] = "\ue25b";
        dict["WrapText"] = "\ue25b";
        dict["wrist"] = "\uf69c";
        dict["wrong_location"] = "\uef78";
        dict["WrongLocation"] = "\uef78";
        dict["wysiwyg"] = "\uf1c3";
        dict["x_circle"] = "\ue349";
        dict["XCircle"] = "\ue349";
        dict["y_circle"] = "\ueec5";
        dict["YCircle"] = "\ueec5";
        dict["yakitori"] = "\uef31";
        dict["yard"] = "\uf089";
        dict["yoshoku"] = "\uf27f";
        dict["your_trips"] = "\ueb2b";
        dict["YourTrips"] = "\ueb2b";
        dict["youtube_activity"] = "\uf85a";
        dict["YoutubeActivity"] = "\uf85a";
        dict["youtube_searched_for"] = "\ue8fa";
        dict["YoutubeSearchedFor"] = "\ue8fa";
        dict["zone_person_alert"] = "\ue781";
        dict["ZonePersonAlert"] = "\ue781";
        dict["zone_person_idle"] = "\ue77a";
        dict["ZonePersonIdle"] = "\ue77a";
        dict["zone_person_urgent"] = "\ue788";
        dict["ZonePersonUrgent"] = "\ue788";
        dict["zoom_in"] = "\ue8ff";
        dict["ZoomIn"] = "\ue8ff";
        dict["zoom_in_map"] = "\ueb2d";
        dict["ZoomInMap"] = "\ueb2d";
        dict["zoom_out"] = "\ue900";
        dict["ZoomOut"] = "\ue900";
        dict["zoom_out_map"] = "\ue56b";
        dict["ZoomOutMap"] = "\ue56b";
        dict["10k"] = "\ue951";
        dict["10mp"] = "\ue952";
        dict["11mp"] = "\ue953";
        dict["123"] = "\ueb8d";
        dict["12mp"] = "\ue954";
        dict["13mp"] = "\ue955";
        dict["14mp"] = "\ue956";
        dict["15mp"] = "\ue957";
        dict["16mp"] = "\ue958";
        dict["17mp"] = "\ue959";
        dict["18_up_rating"] = "\uf8fd";
        dict["_18UpRating"] = "\uf8fd";
        dict["18mp"] = "\ue95a";
        dict["19mp"] = "\ue95b";
        dict["1k"] = "\ue95c";
        dict["1k_plus"] = "\ue95d";
        dict["_1kPlus"] = "\ue95d";
        dict["1x_mobiledata"] = "\uefcd";
        dict["_1xMobiledata"] = "\uefcd";
        dict["1x_mobiledata_badge"] = "\uf7f1";
        dict["_1xMobiledataBadge"] = "\uf7f1";
        dict["20mp"] = "\ue95e";
        dict["21mp"] = "\ue95f";
        dict["22mp"] = "\ue960";
        dict["23mp"] = "\ue961";
        dict["24fps_select"] = "\uf3f2";
        dict["_24fpsSelect"] = "\uf3f2";
        dict["24mp"] = "\ue962";
        dict["2d"] = "\uef37";
        dict["2d_2"] = "\U000FFF0E";
        dict["_2d2"] = "\U000FFF0E";
        dict["2k"] = "\ue963";
        dict["2k_plus"] = "\ue964";
        dict["_2kPlus"] = "\ue964";
        dict["2mp"] = "\ue965";
        dict["30fps"] = "\uefce";
        dict["30fps_select"] = "\uefcf";
        dict["_30fpsSelect"] = "\uefcf";
        dict["360"] = "\ue577";
        dict["3d"] = "\ued38";
        dict["3d_2"] = "\U000FFF0F";
        dict["_3d2"] = "\U000FFF0F";
        dict["3d_rotation"] = "\ue84d";
        dict["_3dRotation"] = "\ue84d";
        dict["3g_mobiledata"] = "\uefd0";
        dict["_3gMobiledata"] = "\uefd0";
        dict["3g_mobiledata_badge"] = "\uf7f0";
        dict["_3gMobiledataBadge"] = "\uf7f0";
        dict["3k"] = "\ue966";
        dict["3k_plus"] = "\ue967";
        dict["_3kPlus"] = "\ue967";
        dict["3mp"] = "\ue968";
        dict["3p"] = "\uefd1";
        dict["4g_mobiledata"] = "\uefd2";
        dict["_4gMobiledata"] = "\uefd2";
        dict["4g_mobiledata_badge"] = "\uf7ef";
        dict["_4gMobiledataBadge"] = "\uf7ef";
        dict["4g_plus_mobiledata"] = "\uefd3";
        dict["_4gPlusMobiledata"] = "\uefd3";
        dict["4k"] = "\ue072";
        dict["4k_plus"] = "\ue969";
        dict["_4kPlus"] = "\ue969";
        dict["4mp"] = "\ue96a";
        dict["50mp"] = "\uf6f3";
        dict["5g"] = "\uef38";
        dict["5g_mobiledata_badge"] = "\uf7ee";
        dict["_5gMobiledataBadge"] = "\uf7ee";
        dict["5k"] = "\ue96b";
        dict["5k_plus"] = "\ue96c";
        dict["_5kPlus"] = "\ue96c";
        dict["5mp"] = "\ue96d";
        dict["60fps"] = "\uefd4";
        dict["60fps_select"] = "\uefd5";
        dict["_60fpsSelect"] = "\uefd5";
        dict["6_ft_apart"] = "\uf21e";
        dict["_6FtApart"] = "\uf21e";
        dict["6k"] = "\ue96e";
        dict["6k_plus"] = "\ue96f";
        dict["_6kPlus"] = "\ue96f";
        dict["6mp"] = "\ue970";
        dict["7k"] = "\ue971";
        dict["7k_plus"] = "\ue972";
        dict["_7kPlus"] = "\ue972";
        dict["7mp"] = "\ue973";
        dict["8k"] = "\ue974";
        dict["8k_plus"] = "\ue975";
        dict["_8kPlus"] = "\ue975";
        dict["8mp"] = "\ue976";
        dict["9k"] = "\ue977";
        dict["9k_plus"] = "\ue978";
        dict["_9kPlus"] = "\ue978";
        dict["9mp"] = "\ue979";
        return dict;
    });

    /// <summary>Resolves a glyph string by its official snake_case or PascalCase name.</summary>
    public static string? GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var clean = name.Trim().Replace("-", "_");
        return _nameToGlyph.Value.TryGetValue(clean, out var glyph) ? Glyph(glyph) : null;
    }

    /// <summary>Total number of icons defined in the official codepoints catalog.</summary>
    public static int TotalCatalogIconCount => 4284;

    /// <summary>Official icon: <c>abc</c></summary>
    public static string? Abc => Glyph("\ueb94");
    /// <summary>Official icon: <c>ac_unit</c></summary>
    public static string? AcUnit => Glyph("\ueb3b");
    /// <summary>Official icon: <c>access_alarm</c></summary>
    public static string? AccessAlarm => Glyph("\ue855");
    /// <summary>Official icon: <c>access_alarms</c></summary>
    public static string? AccessAlarms => Glyph("\ue855");
    /// <summary>Official icon: <c>access_time</c></summary>
    public static string? AccessTime => Glyph("\uefd6");
    /// <summary>Official icon: <c>access_time_filled</c></summary>
    public static string? AccessTimeFilled => Glyph("\uefd6");
    /// <summary>Official icon: <c>accessibility</c></summary>
    public static string? Accessibility => Glyph("\ue84e");
    /// <summary>Official icon: <c>accessibility_new</c></summary>
    public static string? AccessibilityNew => Glyph("\ue92c");
    /// <summary>Official icon: <c>accessible</c></summary>
    public static string? Accessible => Glyph("\ue914");
    /// <summary>Official icon: <c>accessible_forward</c></summary>
    public static string? AccessibleForward => Glyph("\ue934");
    /// <summary>Official icon: <c>accessible_menu</c></summary>
    public static string? AccessibleMenu => Glyph("\uf34e");
    /// <summary>Official icon: <c>account_balance</c></summary>
    public static string? AccountBalance => Glyph("\ue84f");
    /// <summary>Official icon: <c>account_balance_wallet</c></summary>
    public static string? AccountBalanceWallet => Glyph("\ue850");
    /// <summary>Official icon: <c>account_box</c></summary>
    public static string? AccountBox => Glyph("\ue851");
    /// <summary>Official icon: <c>account_child</c></summary>
    public static string? AccountChild => Glyph("\ue852");
    /// <summary>Official icon: <c>account_child_invert</c></summary>
    public static string? AccountChildInvert => Glyph("\ue659");
    /// <summary>Official icon: <c>account_circle</c></summary>
    public static string? AccountCircle => Glyph("\uf20b");
    /// <summary>Official icon: <c>account_circle_filled</c></summary>
    public static string? AccountCircleFilled => Glyph("\uf20b");
    /// <summary>Official icon: <c>account_circle_off</c></summary>
    public static string? AccountCircleOff => Glyph("\uf7b3");
    /// <summary>Official icon: <c>account_tree</c></summary>
    public static string? AccountTree => Glyph("\ue97a");
    /// <summary>Official icon: <c>action_key</c></summary>
    public static string? ActionKey => Glyph("\uf502");
    /// <summary>Official icon: <c>activity_zone</c></summary>
    public static string? ActivityZone => Glyph("\ue1e6");
    /// <summary>Official icon: <c>acupuncture</c></summary>
    public static string? Acupuncture => Glyph("\uf2c4");
    /// <summary>Official icon: <c>acute</c></summary>
    public static string? Acute => Glyph("\ue4cb");
    /// <summary>Official icon: <c>ad</c></summary>
    public static string? Ad => Glyph("\ue65a");
    /// <summary>Official icon: <c>ad_group</c></summary>
    public static string? AdGroup => Glyph("\ue65b");
    /// <summary>Official icon: <c>ad_group_off</c></summary>
    public static string? AdGroupOff => Glyph("\ueae5");
    /// <summary>Official icon: <c>ad_off</c></summary>
    public static string? AdOff => Glyph("\uf7b2");
    /// <summary>Official icon: <c>ad_units</c></summary>
    public static string? AdUnits => Glyph("\uf2eb");
    /// <summary>Official icon: <c>adaptive_audio_mic</c></summary>
    public static string? AdaptiveAudioMic => Glyph("\uf4cc");
    /// <summary>Official icon: <c>adaptive_audio_mic_off</c></summary>
    public static string? AdaptiveAudioMicOff => Glyph("\uf4cb");
    /// <summary>Official icon: <c>adb</c></summary>
    public static string? Adb => Glyph("\ue60e");
    /// <summary>Official icon: <c>add</c></summary>
    public static string? Add => Glyph("\ue145");
    /// <summary>Official icon: <c>add_2</c></summary>
    public static string? Add2 => Glyph("\uf3dd");
    /// <summary>Official icon: <c>add_a_photo</c></summary>
    public static string? AddAPhoto => Glyph("\ue439");
    /// <summary>Official icon: <c>add_ad</c></summary>
    public static string? AddAd => Glyph("\ue72a");
    /// <summary>Official icon: <c>add_alarm</c></summary>
    public static string? AddAlarm => Glyph("\ue856");
    /// <summary>Official icon: <c>add_alert</c></summary>
    public static string? AddAlert => Glyph("\ue003");
    /// <summary>Official icon: <c>add_box</c></summary>
    public static string? AddBox => Glyph("\ue146");
    /// <summary>Official icon: <c>add_business</c></summary>
    public static string? AddBusiness => Glyph("\ue729");
    /// <summary>Official icon: <c>add_call</c></summary>
    public static string? AddCall => Glyph("\uf0b7");
    /// <summary>Official icon: <c>add_card</c></summary>
    public static string? AddCard => Glyph("\ueb86");
    /// <summary>Official icon: <c>add_chart</c></summary>
    public static string? AddChart => Glyph("\uef3c");
    /// <summary>Official icon: <c>add_circle</c></summary>
    public static string? AddCircle => Glyph("\ue990");
    /// <summary>Official icon: <c>add_circle_outline</c></summary>
    public static string? AddCircleOutline => Glyph("\ue990");
    /// <summary>Official icon: <c>add_column_left</c></summary>
    public static string? AddColumnLeft => Glyph("\uf425");
    /// <summary>Official icon: <c>add_column_right</c></summary>
    public static string? AddColumnRight => Glyph("\uf424");
    /// <summary>Official icon: <c>add_comment</c></summary>
    public static string? AddComment => Glyph("\ue266");
    /// <summary>Official icon: <c>add_diamond</c></summary>
    public static string? AddDiamond => Glyph("\uf49c");
    /// <summary>Official icon: <c>add_home</c></summary>
    public static string? AddHome => Glyph("\uf8eb");
    /// <summary>Official icon: <c>add_home_work</c></summary>
    public static string? AddHomeWork => Glyph("\uf8ed");
    /// <summary>Official icon: <c>add_ic_call</c></summary>
    public static string? AddIcCall => Glyph("\uf0b7");
    /// <summary>Official icon: <c>add_link</c></summary>
    public static string? AddLink => Glyph("\ue178");
    /// <summary>Official icon: <c>add_location</c></summary>
    public static string? AddLocation => Glyph("\ue567");
    /// <summary>Official icon: <c>add_location_alt</c></summary>
    public static string? AddLocationAlt => Glyph("\uef3a");
    /// <summary>Official icon: <c>add_moderator</c></summary>
    public static string? AddModerator => Glyph("\ue97d");
    /// <summary>Official icon: <c>add_notes</c></summary>
    public static string? AddNotes => Glyph("\ue091");
    /// <summary>Official icon: <c>add_photo_alternate</c></summary>
    public static string? AddPhotoAlternate => Glyph("\ue43e");
    /// <summary>Official icon: <c>add_reaction</c></summary>
    public static string? AddReaction => Glyph("\ue1d3");
    /// <summary>Official icon: <c>add_road</c></summary>
    public static string? AddRoad => Glyph("\uef3b");
    /// <summary>Official icon: <c>add_row_above</c></summary>
    public static string? AddRowAbove => Glyph("\uf423");
    /// <summary>Official icon: <c>add_row_below</c></summary>
    public static string? AddRowBelow => Glyph("\uf422");
    /// <summary>Official icon: <c>add_shopping_cart</c></summary>
    public static string? AddShoppingCart => Glyph("\ue854");
    /// <summary>Official icon: <c>add_task</c></summary>
    public static string? AddTask => Glyph("\uf23a");
    /// <summary>Official icon: <c>add_to_drive</c></summary>
    public static string? AddToDrive => Glyph("\ue65c");
    /// <summary>Official icon: <c>add_to_home_screen</c></summary>
    public static string? AddToHomeScreen => Glyph("\uf2b9");
    /// <summary>Official icon: <c>add_to_photos</c></summary>
    public static string? AddToPhotos => Glyph("\ue39d");
    /// <summary>Official icon: <c>add_to_queue</c></summary>
    public static string? AddToQueue => Glyph("\ue05c");
    /// <summary>Official icon: <c>add_triangle</c></summary>
    public static string? AddTriangle => Glyph("\uf48e");
    /// <summary>Official icon: <c>addchart</c></summary>
    public static string? Addchart => Glyph("\uef3c");
    /// <summary>Official icon: <c>adf_scanner</c></summary>
    public static string? AdfScanner => Glyph("\ueada");
    /// <summary>Official icon: <c>adjust</c></summary>
    public static string? Adjust => Glyph("\ue39e");
    /// <summary>Official icon: <c>admin_meds</c></summary>
    public static string? AdminMeds => Glyph("\ue48d");
    /// <summary>Official icon: <c>admin_panel_settings</c></summary>
    public static string? AdminPanelSettings => Glyph("\uef3d");
    /// <summary>Official icon: <c>ads_click</c></summary>
    public static string? AdsClick => Glyph("\ue762");
    /// <summary>Official icon: <c>agender</c></summary>
    public static string? Agender => Glyph("\uf888");
    /// <summary>Official icon: <c>agriculture</c></summary>
    public static string? Agriculture => Glyph("\uea79");
    /// <summary>Official icon: <c>air</c></summary>
    public static string? Air => Glyph("\uefd8");
    /// <summary>Official icon: <c>air_freshener</c></summary>
    public static string? AirFreshener => Glyph("\ue2ca");
    /// <summary>Official icon: <c>air_purifier</c></summary>
    public static string? AirPurifier => Glyph("\ue97e");
    /// <summary>Official icon: <c>air_purifier_gen</c></summary>
    public static string? AirPurifierGen => Glyph("\ue829");
    /// <summary>Official icon: <c>airline_seat_flat</c></summary>
    public static string? AirlineSeatFlat => Glyph("\ue630");
    /// <summary>Official icon: <c>airline_seat_flat_angled</c></summary>
    public static string? AirlineSeatFlatAngled => Glyph("\ue631");
    /// <summary>Official icon: <c>airline_seat_individual_suite</c></summary>
    public static string? AirlineSeatIndividualSuite => Glyph("\ue632");
    /// <summary>Official icon: <c>airline_seat_legroom_extra</c></summary>
    public static string? AirlineSeatLegroomExtra => Glyph("\ue633");
    /// <summary>Official icon: <c>airline_seat_legroom_normal</c></summary>
    public static string? AirlineSeatLegroomNormal => Glyph("\ue634");
    /// <summary>Official icon: <c>airline_seat_legroom_reduced</c></summary>
    public static string? AirlineSeatLegroomReduced => Glyph("\ue635");
    /// <summary>Official icon: <c>airline_seat_recline_extra</c></summary>
    public static string? AirlineSeatReclineExtra => Glyph("\ue636");
    /// <summary>Official icon: <c>airline_seat_recline_normal</c></summary>
    public static string? AirlineSeatReclineNormal => Glyph("\ue637");
    /// <summary>Official icon: <c>airline_stops</c></summary>
    public static string? AirlineStops => Glyph("\ue7d0");
    /// <summary>Official icon: <c>airlines</c></summary>
    public static string? Airlines => Glyph("\ue7ca");
    /// <summary>Official icon: <c>airplane_ticket</c></summary>
    public static string? AirplaneTicket => Glyph("\uefd9");
    /// <summary>Official icon: <c>airplanemode_active</c></summary>
    public static string? AirplanemodeActive => Glyph("\ue53d");
    /// <summary>Official icon: <c>airplanemode_inactive</c></summary>
    public static string? AirplanemodeInactive => Glyph("\ue194");
    /// <summary>Official icon: <c>airplay</c></summary>
    public static string? Airplay => Glyph("\ue055");
    /// <summary>Official icon: <c>airport_shuttle</c></summary>
    public static string? AirportShuttle => Glyph("\ueb3c");
    /// <summary>Official icon: <c>airware</c></summary>
    public static string? Airware => Glyph("\uf154");
    /// <summary>Official icon: <c>airwave</c></summary>
    public static string? Airwave => Glyph("\uf154");
    /// <summary>Official icon: <c>alarm</c></summary>
    public static string? Alarm => Glyph("\ue855");
    /// <summary>Official icon: <c>alarm_add</c></summary>
    public static string? AlarmAdd => Glyph("\ue856");
    /// <summary>Official icon: <c>alarm_off</c></summary>
    public static string? AlarmOff => Glyph("\ue857");
    /// <summary>Official icon: <c>alarm_on</c></summary>
    public static string? AlarmOn => Glyph("\ue858");
    /// <summary>Official icon: <c>alarm_pause</c></summary>
    public static string? AlarmPause => Glyph("\uf35b");
    /// <summary>Official icon: <c>alarm_smart_wake</c></summary>
    public static string? AlarmSmartWake => Glyph("\uf6b0");
    /// <summary>Official icon: <c>album</c></summary>
    public static string? Album => Glyph("\ue019");
    /// <summary>Official icon: <c>align_center</c></summary>
    public static string? AlignCenter => Glyph("\ue356");
    /// <summary>Official icon: <c>align_end</c></summary>
    public static string? AlignEnd => Glyph("\uf797");
    /// <summary>Official icon: <c>align_flex_center</c></summary>
    public static string? AlignFlexCenter => Glyph("\uf796");
    /// <summary>Official icon: <c>align_flex_end</c></summary>
    public static string? AlignFlexEnd => Glyph("\uf795");
    /// <summary>Official icon: <c>align_flex_start</c></summary>
    public static string? AlignFlexStart => Glyph("\uf794");
    /// <summary>Official icon: <c>align_horizontal_center</c></summary>
    public static string? AlignHorizontalCenter => Glyph("\ue00f");
    /// <summary>Official icon: <c>align_horizontal_left</c></summary>
    public static string? AlignHorizontalLeft => Glyph("\ue00d");
    /// <summary>Official icon: <c>align_horizontal_right</c></summary>
    public static string? AlignHorizontalRight => Glyph("\ue010");
    /// <summary>Official icon: <c>align_items_stretch</c></summary>
    public static string? AlignItemsStretch => Glyph("\uf793");
    /// <summary>Official icon: <c>align_justify_center</c></summary>
    public static string? AlignJustifyCenter => Glyph("\uf792");
    /// <summary>Official icon: <c>align_justify_flex_end</c></summary>
    public static string? AlignJustifyFlexEnd => Glyph("\uf791");
    /// <summary>Official icon: <c>align_justify_flex_start</c></summary>
    public static string? AlignJustifyFlexStart => Glyph("\uf790");
    /// <summary>Official icon: <c>align_justify_space_around</c></summary>
    public static string? AlignJustifySpaceAround => Glyph("\uf78f");
    /// <summary>Official icon: <c>align_justify_space_between</c></summary>
    public static string? AlignJustifySpaceBetween => Glyph("\uf78e");
    /// <summary>Official icon: <c>align_justify_space_even</c></summary>
    public static string? AlignJustifySpaceEven => Glyph("\uf78d");
    /// <summary>Official icon: <c>align_justify_stretch</c></summary>
    public static string? AlignJustifyStretch => Glyph("\uf78c");
    /// <summary>Official icon: <c>align_self_stretch</c></summary>
    public static string? AlignSelfStretch => Glyph("\uf78b");
    /// <summary>Official icon: <c>align_space_around</c></summary>
    public static string? AlignSpaceAround => Glyph("\uf78a");
    /// <summary>Official icon: <c>align_space_between</c></summary>
    public static string? AlignSpaceBetween => Glyph("\uf789");
    /// <summary>Official icon: <c>align_space_even</c></summary>
    public static string? AlignSpaceEven => Glyph("\uf788");
    /// <summary>Official icon: <c>align_start</c></summary>
    public static string? AlignStart => Glyph("\uf787");
    /// <summary>Official icon: <c>align_stretch</c></summary>
    public static string? AlignStretch => Glyph("\uf786");
    /// <summary>Official icon: <c>align_vertical_bottom</c></summary>
    public static string? AlignVerticalBottom => Glyph("\ue015");
    /// <summary>Official icon: <c>align_vertical_center</c></summary>
    public static string? AlignVerticalCenter => Glyph("\ue011");
    /// <summary>Official icon: <c>align_vertical_top</c></summary>
    public static string? AlignVerticalTop => Glyph("\ue00c");
    /// <summary>Official icon: <c>all_inbox</c></summary>
    public static string? AllInbox => Glyph("\ue97f");
    /// <summary>Official icon: <c>all_inclusive</c></summary>
    public static string? AllInclusive => Glyph("\ueb3d");
    /// <summary>Official icon: <c>all_match</c></summary>
    public static string? AllMatch => Glyph("\ue093");
    /// <summary>Official icon: <c>all_out</c></summary>
    public static string? AllOut => Glyph("\ue90b");
    /// <summary>Official icon: <c>allergies</c></summary>
    public static string? Allergies => Glyph("\ue094");
    /// <summary>Official icon: <c>allergy</c></summary>
    public static string? Allergy => Glyph("\ue64e");
    /// <summary>Official icon: <c>alt_route</c></summary>
    public static string? AltRoute => Glyph("\uf184");
    /// <summary>Official icon: <c>alternate_email</c></summary>
    public static string? AlternateEmail => Glyph("\ue0e6");
    /// <summary>Official icon: <c>altitude</c></summary>
    public static string? Altitude => Glyph("\uf873");
    /// <summary>Official icon: <c>ambient_screen</c></summary>
    public static string? AmbientScreen => Glyph("\uf6c4");
    /// <summary>Official icon: <c>ambulance</c></summary>
    public static string? Ambulance => Glyph("\uf803");
    /// <summary>Official icon: <c>amend</c></summary>
    public static string? Amend => Glyph("\uf802");
    /// <summary>Official icon: <c>amp_stories</c></summary>
    public static string? AmpStories => Glyph("\uea13");
    /// <summary>Official icon: <c>analytics</c></summary>
    public static string? Analytics => Glyph("\uef3e");
    /// <summary>Official icon: <c>anchor</c></summary>
    public static string? Anchor => Glyph("\uf1cd");
    /// <summary>Official icon: <c>android</c></summary>
    public static string? Android => Glyph("\ue859");
    /// <summary>Official icon: <c>android_cell_4_bar</c></summary>
    public static string? AndroidCell4Bar => Glyph("\uef06");
    /// <summary>Official icon: <c>android_cell_4_bar_alert</c></summary>
    public static string? AndroidCell4BarAlert => Glyph("\uef09");
    /// <summary>Official icon: <c>android_cell_4_bar_off</c></summary>
    public static string? AndroidCell4BarOff => Glyph("\uef08");
    /// <summary>Official icon: <c>android_cell_4_bar_plus</c></summary>
    public static string? AndroidCell4BarPlus => Glyph("\uef07");
    /// <summary>Official icon: <c>android_cell_5_bar</c></summary>
    public static string? AndroidCell5Bar => Glyph("\uef02");
    /// <summary>Official icon: <c>android_cell_5_bar_alert</c></summary>
    public static string? AndroidCell5BarAlert => Glyph("\uef05");
    /// <summary>Official icon: <c>android_cell_5_bar_off</c></summary>
    public static string? AndroidCell5BarOff => Glyph("\uef04");
    /// <summary>Official icon: <c>android_cell_5_bar_plus</c></summary>
    public static string? AndroidCell5BarPlus => Glyph("\uef03");
    /// <summary>Official icon: <c>android_cell_dual_4_bar</c></summary>
    public static string? AndroidCellDual4Bar => Glyph("\uef0d");
    /// <summary>Official icon: <c>android_cell_dual_4_bar_alert</c></summary>
    public static string? AndroidCellDual4BarAlert => Glyph("\uef0f");
    /// <summary>Official icon: <c>android_cell_dual_4_bar_plus</c></summary>
    public static string? AndroidCellDual4BarPlus => Glyph("\uef0e");
    /// <summary>Official icon: <c>android_cell_dual_5_bar</c></summary>
    public static string? AndroidCellDual5Bar => Glyph("\uef0a");
    /// <summary>Official icon: <c>android_cell_dual_5_bar_alert</c></summary>
    public static string? AndroidCellDual5BarAlert => Glyph("\uef0c");
    /// <summary>Official icon: <c>android_cell_dual_5_bar_plus</c></summary>
    public static string? AndroidCellDual5BarPlus => Glyph("\uef0b");
    /// <summary>Official icon: <c>android_wifi_3_bar</c></summary>
    public static string? AndroidWifi3Bar => Glyph("\uef16");
    /// <summary>Official icon: <c>android_wifi_3_bar_alert</c></summary>
    public static string? AndroidWifi3BarAlert => Glyph("\uef1b");
    /// <summary>Official icon: <c>android_wifi_3_bar_lock</c></summary>
    public static string? AndroidWifi3BarLock => Glyph("\uef1a");
    /// <summary>Official icon: <c>android_wifi_3_bar_off</c></summary>
    public static string? AndroidWifi3BarOff => Glyph("\uef19");
    /// <summary>Official icon: <c>android_wifi_3_bar_plus</c></summary>
    public static string? AndroidWifi3BarPlus => Glyph("\uef18");
    /// <summary>Official icon: <c>android_wifi_3_bar_question</c></summary>
    public static string? AndroidWifi3BarQuestion => Glyph("\uef17");
    /// <summary>Official icon: <c>android_wifi_4_bar</c></summary>
    public static string? AndroidWifi4Bar => Glyph("\uef10");
    /// <summary>Official icon: <c>android_wifi_4_bar_alert</c></summary>
    public static string? AndroidWifi4BarAlert => Glyph("\uef15");
    /// <summary>Official icon: <c>android_wifi_4_bar_lock</c></summary>
    public static string? AndroidWifi4BarLock => Glyph("\uef14");
    /// <summary>Official icon: <c>android_wifi_4_bar_off</c></summary>
    public static string? AndroidWifi4BarOff => Glyph("\uef13");
    /// <summary>Official icon: <c>android_wifi_4_bar_plus</c></summary>
    public static string? AndroidWifi4BarPlus => Glyph("\uef12");
    /// <summary>Official icon: <c>android_wifi_4_bar_question</c></summary>
    public static string? AndroidWifi4BarQuestion => Glyph("\uef11");
    /// <summary>Official icon: <c>animated_images</c></summary>
    public static string? AnimatedImages => Glyph("\uf49a");
    /// <summary>Official icon: <c>animation</c></summary>
    public static string? Animation => Glyph("\ue71c");
    /// <summary>Official icon: <c>announcement</c></summary>
    public static string? Announcement => Glyph("\ue87f");
    /// <summary>Official icon: <c>antigravity</c></summary>
    public static string? Antigravity => Glyph("\U000FFFD2");
    /// <summary>Official icon: <c>aod</c></summary>
    public static string? Aod => Glyph("\uf2e6");
    /// <summary>Official icon: <c>aod_tablet</c></summary>
    public static string? AodTablet => Glyph("\uf89f");
    /// <summary>Official icon: <c>aod_watch</c></summary>
    public static string? AodWatch => Glyph("\uf6ac");
    /// <summary>Official icon: <c>apartment</c></summary>
    public static string? Apartment => Glyph("\uea40");
    /// <summary>Official icon: <c>api</c></summary>
    public static string? Api => Glyph("\uf1b7");
    /// <summary>Official icon: <c>apk_document</c></summary>
    public static string? ApkDocument => Glyph("\uf88e");
    /// <summary>Official icon: <c>apk_install</c></summary>
    public static string? ApkInstall => Glyph("\uf88f");
    /// <summary>Official icon: <c>app_badging</c></summary>
    public static string? AppBadging => Glyph("\uf72f");
    /// <summary>Official icon: <c>app_blocking</c></summary>
    public static string? AppBlocking => Glyph("\uf2e5");
    /// <summary>Official icon: <c>app_promo</c></summary>
    public static string? AppPromo => Glyph("\uf2cd");
    /// <summary>Official icon: <c>app_registration</c></summary>
    public static string? AppRegistration => Glyph("\uef40");
    /// <summary>Official icon: <c>app_settings_alt</c></summary>
    public static string? AppSettingsAlt => Glyph("\uf2d9");
    /// <summary>Official icon: <c>app_shortcut</c></summary>
    public static string? AppShortcut => Glyph("\uf2df");
    /// <summary>Official icon: <c>apparel</c></summary>
    public static string? Apparel => Glyph("\uef7b");
    /// <summary>Official icon: <c>approval</c></summary>
    public static string? Approval => Glyph("\ue982");
    /// <summary>Official icon: <c>approval_delegation</c></summary>
    public static string? ApprovalDelegation => Glyph("\uf84a");
    /// <summary>Official icon: <c>approval_delegation_off</c></summary>
    public static string? ApprovalDelegationOff => Glyph("\uf2c5");
    /// <summary>Official icon: <c>apps</c></summary>
    public static string? Apps => Glyph("\ue5c3");
    /// <summary>Official icon: <c>apps_outage</c></summary>
    public static string? AppsOutage => Glyph("\ue7cc");
    /// <summary>Official icon: <c>aq</c></summary>
    public static string? Aq => Glyph("\uf55a");
    /// <summary>Official icon: <c>aq_indoor</c></summary>
    public static string? AqIndoor => Glyph("\uf55b");
    /// <summary>Official icon: <c>ar_on_you</c></summary>
    public static string? ArOnYou => Glyph("\uef7c");
    /// <summary>Official icon: <c>ar_stickers</c></summary>
    public static string? ArStickers => Glyph("\ue983");
    /// <summary>Official icon: <c>architecture</c></summary>
    public static string? Architecture => Glyph("\uea3b");
    /// <summary>Official icon: <c>archive</c></summary>
    public static string? Archive => Glyph("\ue149");
    /// <summary>Official icon: <c>area_chart</c></summary>
    public static string? AreaChart => Glyph("\ue770");
    /// <summary>Official icon: <c>arming_countdown</c></summary>
    public static string? ArmingCountdown => Glyph("\ue78a");
    /// <summary>Official icon: <c>arrow_and_edge</c></summary>
    public static string? ArrowAndEdge => Glyph("\uf5d7");
    /// <summary>Official icon: <c>arrow_back</c></summary>
    public static string? ArrowBack => Glyph("\ue5c4");
    /// <summary>Official icon: <c>arrow_back_2</c></summary>
    public static string? ArrowBack2 => Glyph("\uf43a");
    /// <summary>Official icon: <c>arrow_back_ios</c></summary>
    public static string? ArrowBackIos => Glyph("\ue5e0");
    /// <summary>Official icon: <c>arrow_back_ios_new</c></summary>
    public static string? ArrowBackIosNew => Glyph("\ue2ea");
    /// <summary>Official icon: <c>arrow_circle_down</c></summary>
    public static string? ArrowCircleDown => Glyph("\uf181");
    /// <summary>Official icon: <c>arrow_circle_left</c></summary>
    public static string? ArrowCircleLeft => Glyph("\ueaa7");
    /// <summary>Official icon: <c>arrow_circle_right</c></summary>
    public static string? ArrowCircleRight => Glyph("\ueaaa");
    /// <summary>Official icon: <c>arrow_circle_up</c></summary>
    public static string? ArrowCircleUp => Glyph("\uf182");
    /// <summary>Official icon: <c>arrow_cool_down</c></summary>
    public static string? ArrowCoolDown => Glyph("\uf4b6");
    /// <summary>Official icon: <c>arrow_downward</c></summary>
    public static string? ArrowDownward => Glyph("\ue5db");
    /// <summary>Official icon: <c>arrow_downward_alt</c></summary>
    public static string? ArrowDownwardAlt => Glyph("\ue984");
    /// <summary>Official icon: <c>arrow_drop_down</c></summary>
    public static string? ArrowDropDown => Glyph("\ue5c5");
    /// <summary>Official icon: <c>arrow_drop_down_circle</c></summary>
    public static string? ArrowDropDownCircle => Glyph("\ue5c6");
    /// <summary>Official icon: <c>arrow_drop_up</c></summary>
    public static string? ArrowDropUp => Glyph("\ue5c7");
    /// <summary>Official icon: <c>arrow_forward</c></summary>
    public static string? ArrowForward => Glyph("\ue5c8");
    /// <summary>Official icon: <c>arrow_forward_ios</c></summary>
    public static string? ArrowForwardIos => Glyph("\ue5e1");
    /// <summary>Official icon: <c>arrow_insert</c></summary>
    public static string? ArrowInsert => Glyph("\uf837");
    /// <summary>Official icon: <c>arrow_left</c></summary>
    public static string? ArrowLeft => Glyph("\ue5de");
    /// <summary>Official icon: <c>arrow_left_alt</c></summary>
    public static string? ArrowLeftAlt => Glyph("\uef7d");
    /// <summary>Official icon: <c>arrow_menu_close</c></summary>
    public static string? ArrowMenuClose => Glyph("\uf3d3");
    /// <summary>Official icon: <c>arrow_menu_open</c></summary>
    public static string? ArrowMenuOpen => Glyph("\uf3d2");
    /// <summary>Official icon: <c>arrow_or_edge</c></summary>
    public static string? ArrowOrEdge => Glyph("\uf5d6");
    /// <summary>Official icon: <c>arrow_outward</c></summary>
    public static string? ArrowOutward => Glyph("\uf8ce");
    /// <summary>Official icon: <c>arrow_range</c></summary>
    public static string? ArrowRange => Glyph("\uf69b");
    /// <summary>Official icon: <c>arrow_right</c></summary>
    public static string? ArrowRight => Glyph("\ue5df");
    /// <summary>Official icon: <c>arrow_right_alt</c></summary>
    public static string? ArrowRightAlt => Glyph("\ue941");
    /// <summary>Official icon: <c>arrow_selector_tool</c></summary>
    public static string? ArrowSelectorTool => Glyph("\uf82f");
    /// <summary>Official icon: <c>arrow_shape_up</c></summary>
    public static string? ArrowShapeUp => Glyph("\ueef6");
    /// <summary>Official icon: <c>arrow_shape_up_stack</c></summary>
    public static string? ArrowShapeUpStack => Glyph("\ueef7");
    /// <summary>Official icon: <c>arrow_shape_up_stack_2</c></summary>
    public static string? ArrowShapeUpStack2 => Glyph("\ueef8");
    /// <summary>Official icon: <c>arrow_split</c></summary>
    public static string? ArrowSplit => Glyph("\uea04");
    /// <summary>Official icon: <c>arrow_top_left</c></summary>
    public static string? ArrowTopLeft => Glyph("\uf72e");
    /// <summary>Official icon: <c>arrow_top_right</c></summary>
    public static string? ArrowTopRight => Glyph("\uf72d");
    /// <summary>Official icon: <c>arrow_upload_progress</c></summary>
    public static string? ArrowUploadProgress => Glyph("\uf3f4");
    /// <summary>Official icon: <c>arrow_upload_ready</c></summary>
    public static string? ArrowUploadReady => Glyph("\uf3f5");
    /// <summary>Official icon: <c>arrow_upward</c></summary>
    public static string? ArrowUpward => Glyph("\ue5d8");
    /// <summary>Official icon: <c>arrow_upward_alt</c></summary>
    public static string? ArrowUpwardAlt => Glyph("\ue986");
    /// <summary>Official icon: <c>arrow_warm_up</c></summary>
    public static string? ArrowWarmUp => Glyph("\uf4b5");
    /// <summary>Official icon: <c>arrows_input</c></summary>
    public static string? ArrowsInput => Glyph("\uf394");
    /// <summary>Official icon: <c>arrows_left_right_circle</c></summary>
    public static string? ArrowsLeftRightCircle => Glyph("\ueee4");
    /// <summary>Official icon: <c>arrows_more_down</c></summary>
    public static string? ArrowsMoreDown => Glyph("\uf8ab");
    /// <summary>Official icon: <c>arrows_more_up</c></summary>
    public static string? ArrowsMoreUp => Glyph("\uf8ac");
    /// <summary>Official icon: <c>arrows_output</c></summary>
    public static string? ArrowsOutput => Glyph("\uf393");
    /// <summary>Official icon: <c>arrows_outward</c></summary>
    public static string? ArrowsOutward => Glyph("\uf72c");
    /// <summary>Official icon: <c>arrows_up_down_circle</c></summary>
    public static string? ArrowsUpDownCircle => Glyph("\ueee3");
    /// <summary>Official icon: <c>art_track</c></summary>
    public static string? ArtTrack => Glyph("\ue060");
    /// <summary>Official icon: <c>article</c></summary>
    public static string? Article => Glyph("\uef42");
    /// <summary>Official icon: <c>article_person</c></summary>
    public static string? ArticlePerson => Glyph("\uf368");
    /// <summary>Official icon: <c>article_shortcut</c></summary>
    public static string? ArticleShortcut => Glyph("\uf587");
    /// <summary>Official icon: <c>artist</c></summary>
    public static string? Artist => Glyph("\ue01a");
    /// <summary>Official icon: <c>aspect_ratio</c></summary>
    public static string? AspectRatio => Glyph("\ue85b");
    /// <summary>Official icon: <c>assessment</c></summary>
    public static string? Assessment => Glyph("\uf0cc");
    /// <summary>Official icon: <c>assignment</c></summary>
    public static string? Assignment => Glyph("\ue85d");
    /// <summary>Official icon: <c>assignment_add</c></summary>
    public static string? AssignmentAdd => Glyph("\uf848");
    /// <summary>Official icon: <c>assignment_globe</c></summary>
    public static string? AssignmentGlobe => Glyph("\ueeec");
    /// <summary>Official icon: <c>assignment_ind</c></summary>
    public static string? AssignmentInd => Glyph("\ue85e");
    /// <summary>Official icon: <c>assignment_late</c></summary>
    public static string? AssignmentLate => Glyph("\ue85f");
    /// <summary>Official icon: <c>assignment_return</c></summary>
    public static string? AssignmentReturn => Glyph("\ue860");
    /// <summary>Official icon: <c>assignment_returned</c></summary>
    public static string? AssignmentReturned => Glyph("\ue861");
    /// <summary>Official icon: <c>assignment_turned_in</c></summary>
    public static string? AssignmentTurnedIn => Glyph("\ue862");
    /// <summary>Official icon: <c>assist_walker</c></summary>
    public static string? AssistWalker => Glyph("\uf8d5");
    /// <summary>Official icon: <c>assistant</c></summary>
    public static string? Assistant => Glyph("\ue39f");
    /// <summary>Official icon: <c>assistant_device</c></summary>
    public static string? AssistantDevice => Glyph("\ue987");
    /// <summary>Official icon: <c>assistant_direction</c></summary>
    public static string? AssistantDirection => Glyph("\ue988");
    /// <summary>Official icon: <c>assistant_navigation</c></summary>
    public static string? AssistantNavigation => Glyph("\ue989");
    /// <summary>Official icon: <c>assistant_on_hub</c></summary>
    public static string? AssistantOnHub => Glyph("\uf6c1");
    /// <summary>Official icon: <c>assistant_photo</c></summary>
    public static string? AssistantPhoto => Glyph("\uf0c6");
    /// <summary>Official icon: <c>assured_workload</c></summary>
    public static string? AssuredWorkload => Glyph("\ueb6f");
    /// <summary>Official icon: <c>asterisk</c></summary>
    public static string? Asterisk => Glyph("\uf525");
    /// <summary>Official icon: <c>astrophotography_auto</c></summary>
    public static string? AstrophotographyAuto => Glyph("\uf1d9");
    /// <summary>Official icon: <c>astrophotography_off</c></summary>
    public static string? AstrophotographyOff => Glyph("\uf1da");
    /// <summary>Official icon: <c>atm</c></summary>
    public static string? Atm => Glyph("\ue573");
    /// <summary>Official icon: <c>atr</c></summary>
    public static string? Atr => Glyph("\uebc7");
    /// <summary>Official icon: <c>attach_email</c></summary>
    public static string? AttachEmail => Glyph("\uea5e");
    /// <summary>Official icon: <c>attach_file</c></summary>
    public static string? AttachFile => Glyph("\ue226");
    /// <summary>Official icon: <c>attach_file_add</c></summary>
    public static string? AttachFileAdd => Glyph("\uf841");
    /// <summary>Official icon: <c>attach_file_off</c></summary>
    public static string? AttachFileOff => Glyph("\uf4d9");
    /// <summary>Official icon: <c>attach_money</c></summary>
    public static string? AttachMoney => Glyph("\ue227");
    /// <summary>Official icon: <c>attachment</c></summary>
    public static string? Attachment => Glyph("\ue2bc");
    /// <summary>Official icon: <c>attractions</c></summary>
    public static string? Attractions => Glyph("\uea52");
    /// <summary>Official icon: <c>attribution</c></summary>
    public static string? Attribution => Glyph("\uefdb");
    /// <summary>Official icon: <c>audio_capture</c></summary>
    public static string? AudioCapture => Glyph("\U000FFF03");
    /// <summary>Official icon: <c>audio_description</c></summary>
    public static string? AudioDescription => Glyph("\uf58c");
    /// <summary>Official icon: <c>audio_file</c></summary>
    public static string? AudioFile => Glyph("\ueb82");
    /// <summary>Official icon: <c>audio_video_receiver</c></summary>
    public static string? AudioVideoReceiver => Glyph("\uf5d3");
    /// <summary>Official icon: <c>audiotrack</c></summary>
    public static string? Audiotrack => Glyph("\ue405");
    /// <summary>Official icon: <c>auto_activity_zone</c></summary>
    public static string? AutoActivityZone => Glyph("\uf8ad");
    /// <summary>Official icon: <c>auto_awesome</c></summary>
    public static string? AutoAwesome => Glyph("\ue65f");
    /// <summary>Official icon: <c>auto_awesome_mosaic</c></summary>
    public static string? AutoAwesomeMosaic => Glyph("\ue660");
    /// <summary>Official icon: <c>auto_awesome_motion</c></summary>
    public static string? AutoAwesomeMotion => Glyph("\ue661");
    /// <summary>Official icon: <c>auto_delete</c></summary>
    public static string? AutoDelete => Glyph("\uea4c");
    /// <summary>Official icon: <c>auto_detect_voice</c></summary>
    public static string? AutoDetectVoice => Glyph("\uf83e");
    /// <summary>Official icon: <c>auto_draw_solid</c></summary>
    public static string? AutoDrawSolid => Glyph("\ue98a");
    /// <summary>Official icon: <c>auto_fix</c></summary>
    public static string? AutoFix => Glyph("\ue663");
    /// <summary>Official icon: <c>auto_fix_high</c></summary>
    public static string? AutoFixHigh => Glyph("\ue663");
    /// <summary>Official icon: <c>auto_fix_normal</c></summary>
    public static string? AutoFixNormal => Glyph("\ue664");
    /// <summary>Official icon: <c>auto_fix_off</c></summary>
    public static string? AutoFixOff => Glyph("\ue665");
    /// <summary>Official icon: <c>auto_graph</c></summary>
    public static string? AutoGraph => Glyph("\ue4fb");
    /// <summary>Official icon: <c>auto_label</c></summary>
    public static string? AutoLabel => Glyph("\uf6be");
    /// <summary>Official icon: <c>auto_meeting_room</c></summary>
    public static string? AutoMeetingRoom => Glyph("\uf6bf");
    /// <summary>Official icon: <c>auto_mode</c></summary>
    public static string? AutoMode => Glyph("\uec20");
    /// <summary>Official icon: <c>auto_read_pause</c></summary>
    public static string? AutoReadPause => Glyph("\uf219");
    /// <summary>Official icon: <c>auto_read_play</c></summary>
    public static string? AutoReadPlay => Glyph("\uf216");
    /// <summary>Official icon: <c>auto_schedule</c></summary>
    public static string? AutoSchedule => Glyph("\ue214");
    /// <summary>Official icon: <c>auto_stories</c></summary>
    public static string? AutoStories => Glyph("\ue666");
    /// <summary>Official icon: <c>auto_stories_off</c></summary>
    public static string? AutoStoriesOff => Glyph("\uf267");
    /// <summary>Official icon: <c>auto_timer</c></summary>
    public static string? AutoTimer => Glyph("\uef7f");
    /// <summary>Official icon: <c>auto_towing</c></summary>
    public static string? AutoTowing => Glyph("\ue71e");
    /// <summary>Official icon: <c>auto_transmission</c></summary>
    public static string? AutoTransmission => Glyph("\uf53f");
    /// <summary>Official icon: <c>auto_videocam</c></summary>
    public static string? AutoVideocam => Glyph("\uf6c0");
    /// <summary>Official icon: <c>autofps_select</c></summary>
    public static string? AutofpsSelect => Glyph("\uefdc");
    /// <summary>Official icon: <c>automation</c></summary>
    public static string? Automation => Glyph("\uf421");
    /// <summary>Official icon: <c>autopause</c></summary>
    public static string? Autopause => Glyph("\uf6b6");
    /// <summary>Official icon: <c>autopay</c></summary>
    public static string? Autopay => Glyph("\uf84b");
    /// <summary>Official icon: <c>autoplay</c></summary>
    public static string? Autoplay => Glyph("\uf6b5");
    /// <summary>Official icon: <c>autorenew</c></summary>
    public static string? Autorenew => Glyph("\ue863");
    /// <summary>Official icon: <c>autostop</c></summary>
    public static string? Autostop => Glyph("\uf682");
    /// <summary>Official icon: <c>av1</c></summary>
    public static string? Av1 => Glyph("\uf4b0");
    /// <summary>Official icon: <c>av_timer</c></summary>
    public static string? AvTimer => Glyph("\ue01b");
    /// <summary>Official icon: <c>avc</c></summary>
    public static string? Avc => Glyph("\uf4af");
    /// <summary>Official icon: <c>avg_pace</c></summary>
    public static string? AvgPace => Glyph("\uf6bb");
    /// <summary>Official icon: <c>avg_time</c></summary>
    public static string? AvgTime => Glyph("\uf813");
    /// <summary>Official icon: <c>avocado_bean</c></summary>
    public static string? AvocadoBean => Glyph("\U000FFFA7");
    /// <summary>Official icon: <c>award_meal</c></summary>
    public static string? AwardMeal => Glyph("\uf241");
    /// <summary>Official icon: <c>award_star</c></summary>
    public static string? AwardStar => Glyph("\uf612");
    /// <summary>Official icon: <c>azm</c></summary>
    public static string? Azm => Glyph("\uf6ec");
    /// <summary>Official icon: <c>b_circle</c></summary>
    public static string? BCircle => Glyph("\ueee2");
    /// <summary>Official icon: <c>baby_changing_station</c></summary>
    public static string? BabyChangingStation => Glyph("\uf19b");
    /// <summary>Official icon: <c>back_hand</c></summary>
    public static string? BackHand => Glyph("\ue764");
    /// <summary>Official icon: <c>back_to_tab</c></summary>
    public static string? BackToTab => Glyph("\uf72b");
    /// <summary>Official icon: <c>background_dot_large</c></summary>
    public static string? BackgroundDotLarge => Glyph("\uf79e");
    /// <summary>Official icon: <c>background_dot_small</c></summary>
    public static string? BackgroundDotSmall => Glyph("\uf514");
    /// <summary>Official icon: <c>background_grid_small</c></summary>
    public static string? BackgroundGridSmall => Glyph("\uf79d");
    /// <summary>Official icon: <c>background_replace</c></summary>
    public static string? BackgroundReplace => Glyph("\uf20a");
    /// <summary>Official icon: <c>backlight_high</c></summary>
    public static string? BacklightHigh => Glyph("\uf7ed");
    /// <summary>Official icon: <c>backlight_high_off</c></summary>
    public static string? BacklightHighOff => Glyph("\uf4ef");
    /// <summary>Official icon: <c>backlight_low</c></summary>
    public static string? BacklightLow => Glyph("\uf7ec");
    /// <summary>Official icon: <c>backpack</c></summary>
    public static string? Backpack => Glyph("\uf19c");
    /// <summary>Official icon: <c>backspace</c></summary>
    public static string? Backspace => Glyph("\ue14a");
    /// <summary>Official icon: <c>backup</c></summary>
    public static string? Backup => Glyph("\ue864");
    /// <summary>Official icon: <c>backup_table</c></summary>
    public static string? BackupTable => Glyph("\uef43");
    /// <summary>Official icon: <c>badge</c></summary>
    public static string? Badge => Glyph("\uea67");
    /// <summary>Official icon: <c>badge_critical_battery</c></summary>
    public static string? BadgeCriticalBattery => Glyph("\uf156");
    /// <summary>Official icon: <c>badminton</c></summary>
    public static string? Badminton => Glyph("\uf2a8");
    /// <summary>Official icon: <c>bakery_dining</c></summary>
    public static string? BakeryDining => Glyph("\uea53");
    /// <summary>Official icon: <c>balance</c></summary>
    public static string? Balance => Glyph("\ueaf6");
    /// <summary>Official icon: <c>balcony</c></summary>
    public static string? Balcony => Glyph("\ue58f");
    /// <summary>Official icon: <c>ballot</c></summary>
    public static string? Ballot => Glyph("\ue172");
    /// <summary>Official icon: <c>bar_chart</c></summary>
    public static string? BarChart => Glyph("\ue26b");
    /// <summary>Official icon: <c>bar_chart_4_bars</c></summary>
    public static string? BarChart4Bars => Glyph("\uf681");
    /// <summary>Official icon: <c>bar_chart_off</c></summary>
    public static string? BarChartOff => Glyph("\uf411");
    /// <summary>Official icon: <c>barcode</c></summary>
    public static string? Barcode => Glyph("\ue70b");
    /// <summary>Official icon: <c>barcode_reader</c></summary>
    public static string? BarcodeReader => Glyph("\uf85c");
    /// <summary>Official icon: <c>barcode_scanner</c></summary>
    public static string? BarcodeScanner => Glyph("\ue70c");
    /// <summary>Official icon: <c>barefoot</c></summary>
    public static string? Barefoot => Glyph("\uf871");
    /// <summary>Official icon: <c>batch_prediction</c></summary>
    public static string? BatchPrediction => Glyph("\uf0f5");
    /// <summary>Official icon: <c>bath_bedrock</c></summary>
    public static string? BathBedrock => Glyph("\uf286");
    /// <summary>Official icon: <c>bath_outdoor</c></summary>
    public static string? BathOutdoor => Glyph("\uf6fb");
    /// <summary>Official icon: <c>bath_private</c></summary>
    public static string? BathPrivate => Glyph("\uf6fa");
    /// <summary>Official icon: <c>bath_public_large</c></summary>
    public static string? BathPublicLarge => Glyph("\uf6f9");
    /// <summary>Official icon: <c>bath_soak</c></summary>
    public static string? BathSoak => Glyph("\uf2a0");
    /// <summary>Official icon: <c>bathroom</c></summary>
    public static string? Bathroom => Glyph("\uefdd");
    /// <summary>Official icon: <c>bathtub</c></summary>
    public static string? Bathtub => Glyph("\uea41");
    /// <summary>Official icon: <c>battery_0_bar</c></summary>
    public static string? Battery0Bar => Glyph("\uebdc");
    /// <summary>Official icon: <c>battery_1_bar</c></summary>
    public static string? Battery1Bar => Glyph("\uf09c");
    /// <summary>Official icon: <c>battery_20</c></summary>
    public static string? Battery20 => Glyph("\uf09c");
    /// <summary>Official icon: <c>battery_2_bar</c></summary>
    public static string? Battery2Bar => Glyph("\uf09d");
    /// <summary>Official icon: <c>battery_30</c></summary>
    public static string? Battery30 => Glyph("\uf09d");
    /// <summary>Official icon: <c>battery_3_bar</c></summary>
    public static string? Battery3Bar => Glyph("\uf09e");
    /// <summary>Official icon: <c>battery_4_bar</c></summary>
    public static string? Battery4Bar => Glyph("\uf09f");
    /// <summary>Official icon: <c>battery_50</c></summary>
    public static string? Battery50 => Glyph("\uf09e");
    /// <summary>Official icon: <c>battery_5_bar</c></summary>
    public static string? Battery5Bar => Glyph("\uf0a0");
    /// <summary>Official icon: <c>battery_60</c></summary>
    public static string? Battery60 => Glyph("\uf09f");
    /// <summary>Official icon: <c>battery_6_bar</c></summary>
    public static string? Battery6Bar => Glyph("\uf0a1");
    /// <summary>Official icon: <c>battery_80</c></summary>
    public static string? Battery80 => Glyph("\uf0a0");
    /// <summary>Official icon: <c>battery_90</c></summary>
    public static string? Battery90 => Glyph("\uf0a1");
    /// <summary>Official icon: <c>battery_alert</c></summary>
    public static string? BatteryAlert => Glyph("\ue19c");
    /// <summary>Official icon: <c>battery_android_0</c></summary>
    public static string? BatteryAndroid0 => Glyph("\uf30d");
    /// <summary>Official icon: <c>battery_android_1</c></summary>
    public static string? BatteryAndroid1 => Glyph("\uf30c");
    /// <summary>Official icon: <c>battery_android_2</c></summary>
    public static string? BatteryAndroid2 => Glyph("\uf30b");
    /// <summary>Official icon: <c>battery_android_3</c></summary>
    public static string? BatteryAndroid3 => Glyph("\uf30a");
    /// <summary>Official icon: <c>battery_android_4</c></summary>
    public static string? BatteryAndroid4 => Glyph("\uf309");
    /// <summary>Official icon: <c>battery_android_5</c></summary>
    public static string? BatteryAndroid5 => Glyph("\uf308");
    /// <summary>Official icon: <c>battery_android_6</c></summary>
    public static string? BatteryAndroid6 => Glyph("\uf307");
    /// <summary>Official icon: <c>battery_android_alert</c></summary>
    public static string? BatteryAndroidAlert => Glyph("\uf306");
    /// <summary>Official icon: <c>battery_android_bolt</c></summary>
    public static string? BatteryAndroidBolt => Glyph("\uf305");
    /// <summary>Official icon: <c>battery_android_frame_1</c></summary>
    public static string? BatteryAndroidFrame1 => Glyph("\uf257");
    /// <summary>Official icon: <c>battery_android_frame_2</c></summary>
    public static string? BatteryAndroidFrame2 => Glyph("\uf256");
    /// <summary>Official icon: <c>battery_android_frame_3</c></summary>
    public static string? BatteryAndroidFrame3 => Glyph("\uf255");
    /// <summary>Official icon: <c>battery_android_frame_4</c></summary>
    public static string? BatteryAndroidFrame4 => Glyph("\uf254");
    /// <summary>Official icon: <c>battery_android_frame_5</c></summary>
    public static string? BatteryAndroidFrame5 => Glyph("\uf253");
    /// <summary>Official icon: <c>battery_android_frame_6</c></summary>
    public static string? BatteryAndroidFrame6 => Glyph("\uf252");
    /// <summary>Official icon: <c>battery_android_frame_alert</c></summary>
    public static string? BatteryAndroidFrameAlert => Glyph("\uf251");
    /// <summary>Official icon: <c>battery_android_frame_bolt</c></summary>
    public static string? BatteryAndroidFrameBolt => Glyph("\uf250");
    /// <summary>Official icon: <c>battery_android_frame_full</c></summary>
    public static string? BatteryAndroidFrameFull => Glyph("\uf24f");
    /// <summary>Official icon: <c>battery_android_frame_plus</c></summary>
    public static string? BatteryAndroidFramePlus => Glyph("\uf24e");
    /// <summary>Official icon: <c>battery_android_frame_question</c></summary>
    public static string? BatteryAndroidFrameQuestion => Glyph("\uf24d");
    /// <summary>Official icon: <c>battery_android_frame_share</c></summary>
    public static string? BatteryAndroidFrameShare => Glyph("\uf24c");
    /// <summary>Official icon: <c>battery_android_frame_shield</c></summary>
    public static string? BatteryAndroidFrameShield => Glyph("\uf24b");
    /// <summary>Official icon: <c>battery_android_full</c></summary>
    public static string? BatteryAndroidFull => Glyph("\uf304");
    /// <summary>Official icon: <c>battery_android_plus</c></summary>
    public static string? BatteryAndroidPlus => Glyph("\uf303");
    /// <summary>Official icon: <c>battery_android_question</c></summary>
    public static string? BatteryAndroidQuestion => Glyph("\uf302");
    /// <summary>Official icon: <c>battery_android_share</c></summary>
    public static string? BatteryAndroidShare => Glyph("\uf301");
    /// <summary>Official icon: <c>battery_android_shield</c></summary>
    public static string? BatteryAndroidShield => Glyph("\uf300");
    /// <summary>Official icon: <c>battery_change</c></summary>
    public static string? BatteryChange => Glyph("\uf7eb");
    /// <summary>Official icon: <c>battery_charging_20</c></summary>
    public static string? BatteryCharging20 => Glyph("\uf0a2");
    /// <summary>Official icon: <c>battery_charging_20_2</c></summary>
    public static string? BatteryCharging202 => Glyph("\U000FFF3E");
    /// <summary>Official icon: <c>battery_charging_30</c></summary>
    public static string? BatteryCharging30 => Glyph("\uf0a3");
    /// <summary>Official icon: <c>battery_charging_30_2</c></summary>
    public static string? BatteryCharging302 => Glyph("\U000FFF3D");
    /// <summary>Official icon: <c>battery_charging_50</c></summary>
    public static string? BatteryCharging50 => Glyph("\uf0a4");
    /// <summary>Official icon: <c>battery_charging_50_2</c></summary>
    public static string? BatteryCharging502 => Glyph("\U000FFF3C");
    /// <summary>Official icon: <c>battery_charging_60</c></summary>
    public static string? BatteryCharging60 => Glyph("\uf0a5");
    /// <summary>Official icon: <c>battery_charging_60_2</c></summary>
    public static string? BatteryCharging602 => Glyph("\U000FFF3B");
    /// <summary>Official icon: <c>battery_charging_80</c></summary>
    public static string? BatteryCharging80 => Glyph("\uf0a6");
    /// <summary>Official icon: <c>battery_charging_80_2</c></summary>
    public static string? BatteryCharging802 => Glyph("\U000FFF3A");
    /// <summary>Official icon: <c>battery_charging_90</c></summary>
    public static string? BatteryCharging90 => Glyph("\uf0a7");
    /// <summary>Official icon: <c>battery_charging_full</c></summary>
    public static string? BatteryChargingFull => Glyph("\ue1a3");
    /// <summary>Official icon: <c>battery_charging_full_2</c></summary>
    public static string? BatteryChargingFull2 => Glyph("\U000FFF39");
    /// <summary>Official icon: <c>battery_error</c></summary>
    public static string? BatteryError => Glyph("\uf7ea");
    /// <summary>Official icon: <c>battery_full</c></summary>
    public static string? BatteryFull => Glyph("\ue1a5");
    /// <summary>Official icon: <c>battery_full_alt</c></summary>
    public static string? BatteryFullAlt => Glyph("\uf13b");
    /// <summary>Official icon: <c>battery_horiz_000</c></summary>
    public static string? BatteryHoriz000 => Glyph("\uf8ae");
    /// <summary>Official icon: <c>battery_horiz_050</c></summary>
    public static string? BatteryHoriz050 => Glyph("\uf8af");
    /// <summary>Official icon: <c>battery_horiz_075</c></summary>
    public static string? BatteryHoriz075 => Glyph("\uf8b0");
    /// <summary>Official icon: <c>battery_low</c></summary>
    public static string? BatteryLow => Glyph("\uf155");
    /// <summary>Official icon: <c>battery_plus</c></summary>
    public static string? BatteryPlus => Glyph("\uf7e9");
    /// <summary>Official icon: <c>battery_profile</c></summary>
    public static string? BatteryProfile => Glyph("\ue206");
    /// <summary>Official icon: <c>battery_saver</c></summary>
    public static string? BatterySaver => Glyph("\uefde");
    /// <summary>Official icon: <c>battery_share</c></summary>
    public static string? BatteryShare => Glyph("\uf67e");
    /// <summary>Official icon: <c>battery_status_good</c></summary>
    public static string? BatteryStatusGood => Glyph("\uf67d");
    /// <summary>Official icon: <c>battery_std</c></summary>
    public static string? BatteryStd => Glyph("\ue1a5");
    /// <summary>Official icon: <c>battery_unknown</c></summary>
    public static string? BatteryUnknown => Glyph("\ue1a6");
    /// <summary>Official icon: <c>battery_vert_005</c></summary>
    public static string? BatteryVert005 => Glyph("\uf8b1");
    /// <summary>Official icon: <c>battery_vert_020</c></summary>
    public static string? BatteryVert020 => Glyph("\uf8b2");
    /// <summary>Official icon: <c>battery_vert_050</c></summary>
    public static string? BatteryVert050 => Glyph("\uf8b3");
    /// <summary>Official icon: <c>battery_very_low</c></summary>
    public static string? BatteryVeryLow => Glyph("\uf156");
    /// <summary>Official icon: <c>beach_access</c></summary>
    public static string? BeachAccess => Glyph("\ueb3e");
    /// <summary>Official icon: <c>bed</c></summary>
    public static string? Bed => Glyph("\uefdf");
    /// <summary>Official icon: <c>bedroom_baby</c></summary>
    public static string? BedroomBaby => Glyph("\uefe0");
    /// <summary>Official icon: <c>bedroom_child</c></summary>
    public static string? BedroomChild => Glyph("\uefe1");
    /// <summary>Official icon: <c>bedroom_parent</c></summary>
    public static string? BedroomParent => Glyph("\uefe2");
    /// <summary>Official icon: <c>bedtime</c></summary>
    public static string? Bedtime => Glyph("\uf159");
    /// <summary>Official icon: <c>bedtime_off</c></summary>
    public static string? BedtimeOff => Glyph("\ueb76");
    /// <summary>Official icon: <c>beenhere</c></summary>
    public static string? Beenhere => Glyph("\ue52d");
    /// <summary>Official icon: <c>beer_meal</c></summary>
    public static string? BeerMeal => Glyph("\uf285");
    /// <summary>Official icon: <c>bento</c></summary>
    public static string? Bento => Glyph("\uf1f4");
    /// <summary>Official icon: <c>bia</c></summary>
    public static string? Bia => Glyph("\uf6eb");
    /// <summary>Official icon: <c>bid_landscape</c></summary>
    public static string? BidLandscape => Glyph("\ue667");
    /// <summary>Official icon: <c>bid_landscape_disabled</c></summary>
    public static string? BidLandscapeDisabled => Glyph("\uef81");
    /// <summary>Official icon: <c>bigtop_updates</c></summary>
    public static string? BigtopUpdates => Glyph("\ue669");
    /// <summary>Official icon: <c>bike_dock</c></summary>
    public static string? BikeDock => Glyph("\uf47b");
    /// <summary>Official icon: <c>bike_lane</c></summary>
    public static string? BikeLane => Glyph("\uf47a");
    /// <summary>Official icon: <c>bike_scooter</c></summary>
    public static string? BikeScooter => Glyph("\uef45");
    /// <summary>Official icon: <c>biotech</c></summary>
    public static string? Biotech => Glyph("\uea3a");
    /// <summary>Official icon: <c>blanket</c></summary>
    public static string? Blanket => Glyph("\ue828");
    /// <summary>Official icon: <c>blender</c></summary>
    public static string? Blender => Glyph("\uefe3");
    /// <summary>Official icon: <c>blind</c></summary>
    public static string? Blind => Glyph("\uf8d6");
    /// <summary>Official icon: <c>blinds</c></summary>
    public static string? Blinds => Glyph("\ue286");
    /// <summary>Official icon: <c>blinds_2</c></summary>
    public static string? Blinds2 => Glyph("\U000FFF78");
    /// <summary>Official icon: <c>blinds_2_closed</c></summary>
    public static string? Blinds2Closed => Glyph("\U000FFF79");
    /// <summary>Official icon: <c>blinds_closed</c></summary>
    public static string? BlindsClosed => Glyph("\uec1f");
    /// <summary>Official icon: <c>block</c></summary>
    public static string? Block => Glyph("\uf08c");
    /// <summary>Official icon: <c>blood_pressure</c></summary>
    public static string? BloodPressure => Glyph("\ue097");
    /// <summary>Official icon: <c>bloodtype</c></summary>
    public static string? Bloodtype => Glyph("\uefe4");
    /// <summary>Official icon: <c>bluetooth</c></summary>
    public static string? Bluetooth => Glyph("\ue1a7");
    /// <summary>Official icon: <c>bluetooth_audio</c></summary>
    public static string? BluetoothAudio => Glyph("\ue60f");
    /// <summary>Official icon: <c>bluetooth_connected</c></summary>
    public static string? BluetoothConnected => Glyph("\ue1a8");
    /// <summary>Official icon: <c>bluetooth_disabled</c></summary>
    public static string? BluetoothDisabled => Glyph("\ue1a9");
    /// <summary>Official icon: <c>bluetooth_drive</c></summary>
    public static string? BluetoothDrive => Glyph("\uefe5");
    /// <summary>Official icon: <c>bluetooth_searching</c></summary>
    public static string? BluetoothSearching => Glyph("\ue60f");
    /// <summary>Official icon: <c>blur_circular</c></summary>
    public static string? BlurCircular => Glyph("\ue3a2");
    /// <summary>Official icon: <c>blur_linear</c></summary>
    public static string? BlurLinear => Glyph("\ue3a3");
    /// <summary>Official icon: <c>blur_medium</c></summary>
    public static string? BlurMedium => Glyph("\ue84c");
    /// <summary>Official icon: <c>blur_off</c></summary>
    public static string? BlurOff => Glyph("\ue3a4");
    /// <summary>Official icon: <c>blur_on</c></summary>
    public static string? BlurOn => Glyph("\ue3a5");
    /// <summary>Official icon: <c>blur_short</c></summary>
    public static string? BlurShort => Glyph("\ue8cf");
    /// <summary>Official icon: <c>boat_bus</c></summary>
    public static string? BoatBus => Glyph("\uf36d");
    /// <summary>Official icon: <c>boat_railway</c></summary>
    public static string? BoatRailway => Glyph("\uf36c");
    /// <summary>Official icon: <c>body_fat</c></summary>
    public static string? BodyFat => Glyph("\ue098");
    /// <summary>Official icon: <c>body_system</c></summary>
    public static string? BodySystem => Glyph("\ue099");
    /// <summary>Official icon: <c>bolt</c></summary>
    public static string? Bolt => Glyph("\uea0b");
    /// <summary>Official icon: <c>bolt_boost</c></summary>
    public static string? BoltBoost => Glyph("\U000FFF6A");
    /// <summary>Official icon: <c>bomb</c></summary>
    public static string? Bomb => Glyph("\uf568");
    /// <summary>Official icon: <c>book</c></summary>
    public static string? Book => Glyph("\ue86e");
    /// <summary>Official icon: <c>book_2</c></summary>
    public static string? Book2 => Glyph("\uf53e");
    /// <summary>Official icon: <c>book_3</c></summary>
    public static string? Book3 => Glyph("\uf53d");
    /// <summary>Official icon: <c>book_4</c></summary>
    public static string? Book4 => Glyph("\uf53c");
    /// <summary>Official icon: <c>book_5</c></summary>
    public static string? Book5 => Glyph("\uf53b");
    /// <summary>Official icon: <c>book_6</c></summary>
    public static string? Book6 => Glyph("\uf3df");
    /// <summary>Official icon: <c>book_online</c></summary>
    public static string? BookOnline => Glyph("\uf2e4");
    /// <summary>Official icon: <c>book_ribbon</c></summary>
    public static string? BookRibbon => Glyph("\uf3e7");
    /// <summary>Official icon: <c>bookmark</c></summary>
    public static string? Bookmark => Glyph("\ue8e7");
    /// <summary>Official icon: <c>bookmark_add</c></summary>
    public static string? BookmarkAdd => Glyph("\ue598");
    /// <summary>Official icon: <c>bookmark_added</c></summary>
    public static string? BookmarkAdded => Glyph("\ue599");
    /// <summary>Official icon: <c>bookmark_bag</c></summary>
    public static string? BookmarkBag => Glyph("\uf410");
    /// <summary>Official icon: <c>bookmark_border</c></summary>
    public static string? BookmarkBorder => Glyph("\ue8e7");
    /// <summary>Official icon: <c>bookmark_check</c></summary>
    public static string? BookmarkCheck => Glyph("\uf457");
    /// <summary>Official icon: <c>bookmark_flag</c></summary>
    public static string? BookmarkFlag => Glyph("\uf456");
    /// <summary>Official icon: <c>bookmark_heart</c></summary>
    public static string? BookmarkHeart => Glyph("\uf455");
    /// <summary>Official icon: <c>bookmark_manager</c></summary>
    public static string? BookmarkManager => Glyph("\uf7b1");
    /// <summary>Official icon: <c>bookmark_remove</c></summary>
    public static string? BookmarkRemove => Glyph("\ue59a");
    /// <summary>Official icon: <c>bookmark_stacks</c></summary>
    public static string? BookmarkStacks => Glyph("\ueee8");
    /// <summary>Official icon: <c>bookmark_star</c></summary>
    public static string? BookmarkStar => Glyph("\uf454");
    /// <summary>Official icon: <c>bookmarks</c></summary>
    public static string? Bookmarks => Glyph("\ue98b");
    /// <summary>Official icon: <c>books_movies_and_music</c></summary>
    public static string? BooksMoviesAndMusic => Glyph("\uef82");
    /// <summary>Official icon: <c>border_all</c></summary>
    public static string? BorderAll => Glyph("\ue228");
    /// <summary>Official icon: <c>border_bottom</c></summary>
    public static string? BorderBottom => Glyph("\ue229");
    /// <summary>Official icon: <c>border_clear</c></summary>
    public static string? BorderClear => Glyph("\ue22a");
    /// <summary>Official icon: <c>border_color</c></summary>
    public static string? BorderColor => Glyph("\ue22b");
    /// <summary>Official icon: <c>border_horizontal</c></summary>
    public static string? BorderHorizontal => Glyph("\ue22c");
    /// <summary>Official icon: <c>border_inner</c></summary>
    public static string? BorderInner => Glyph("\ue22d");
    /// <summary>Official icon: <c>border_left</c></summary>
    public static string? BorderLeft => Glyph("\ue22e");
    /// <summary>Official icon: <c>border_outer</c></summary>
    public static string? BorderOuter => Glyph("\ue22f");
    /// <summary>Official icon: <c>border_right</c></summary>
    public static string? BorderRight => Glyph("\ue230");
    /// <summary>Official icon: <c>border_style</c></summary>
    public static string? BorderStyle => Glyph("\ue231");
    /// <summary>Official icon: <c>border_top</c></summary>
    public static string? BorderTop => Glyph("\ue232");
    /// <summary>Official icon: <c>border_vertical</c></summary>
    public static string? BorderVertical => Glyph("\ue233");
    /// <summary>Official icon: <c>borg</c></summary>
    public static string? Borg => Glyph("\uf40d");
    /// <summary>Official icon: <c>bottom_app_bar</c></summary>
    public static string? BottomAppBar => Glyph("\ue730");
    /// <summary>Official icon: <c>bottom_drawer</c></summary>
    public static string? BottomDrawer => Glyph("\ue72d");
    /// <summary>Official icon: <c>bottom_navigation</c></summary>
    public static string? BottomNavigation => Glyph("\ue98c");
    /// <summary>Official icon: <c>bottom_panel_close</c></summary>
    public static string? BottomPanelClose => Glyph("\uf72a");
    /// <summary>Official icon: <c>bottom_panel_open</c></summary>
    public static string? BottomPanelOpen => Glyph("\uf729");
    /// <summary>Official icon: <c>bottom_right_click</c></summary>
    public static string? BottomRightClick => Glyph("\uf684");
    /// <summary>Official icon: <c>bottom_sheets</c></summary>
    public static string? BottomSheets => Glyph("\ue98d");
    /// <summary>Official icon: <c>box</c></summary>
    public static string? Box => Glyph("\uf5a4");
    /// <summary>Official icon: <c>box_add</c></summary>
    public static string? BoxAdd => Glyph("\uf5a5");
    /// <summary>Official icon: <c>box_edit</c></summary>
    public static string? BoxEdit => Glyph("\uf5a6");
    /// <summary>Official icon: <c>boy</c></summary>
    public static string? Boy => Glyph("\ueb67");
    /// <summary>Official icon: <c>brand_awareness</c></summary>
    public static string? BrandAwareness => Glyph("\ue98e");
    /// <summary>Official icon: <c>brand_family</c></summary>
    public static string? BrandFamily => Glyph("\uf4f1");
    /// <summary>Official icon: <c>branding_watermark</c></summary>
    public static string? BrandingWatermark => Glyph("\ue06b");
    /// <summary>Official icon: <c>breakfast_dining</c></summary>
    public static string? BreakfastDining => Glyph("\uea54");
    /// <summary>Official icon: <c>breaking_news</c></summary>
    public static string? BreakingNews => Glyph("\uea08");
    /// <summary>Official icon: <c>breaking_news_alt_1</c></summary>
    public static string? BreakingNewsAlt1 => Glyph("\uf0ba");
    /// <summary>Official icon: <c>breastfeeding</c></summary>
    public static string? Breastfeeding => Glyph("\uf856");
    /// <summary>Official icon: <c>brick</c></summary>
    public static string? Brick => Glyph("\uf388");
    /// <summary>Official icon: <c>briefcase_meal</c></summary>
    public static string? BriefcaseMeal => Glyph("\uf246");
    /// <summary>Official icon: <c>brightness_1</c></summary>
    public static string? Brightness1 => Glyph("\ue3fa");
    /// <summary>Official icon: <c>brightness_2</c></summary>
    public static string? Brightness2 => Glyph("\uf036");
    /// <summary>Official icon: <c>brightness_3</c></summary>
    public static string? Brightness3 => Glyph("\ue3a8");
    /// <summary>Official icon: <c>brightness_4</c></summary>
    public static string? Brightness4 => Glyph("\ue3a9");
    /// <summary>Official icon: <c>brightness_5</c></summary>
    public static string? Brightness5 => Glyph("\ue3aa");
    /// <summary>Official icon: <c>brightness_6</c></summary>
    public static string? Brightness6 => Glyph("\ue3ab");
    /// <summary>Official icon: <c>brightness_7</c></summary>
    public static string? Brightness7 => Glyph("\ue3ac");
    /// <summary>Official icon: <c>brightness_alert</c></summary>
    public static string? BrightnessAlert => Glyph("\uf5cf");
    /// <summary>Official icon: <c>brightness_auto</c></summary>
    public static string? BrightnessAuto => Glyph("\ue1ab");
    /// <summary>Official icon: <c>brightness_empty</c></summary>
    public static string? BrightnessEmpty => Glyph("\uf7e8");
    /// <summary>Official icon: <c>brightness_high</c></summary>
    public static string? BrightnessHigh => Glyph("\ue1ac");
    /// <summary>Official icon: <c>brightness_low</c></summary>
    public static string? BrightnessLow => Glyph("\ue1ad");
    /// <summary>Official icon: <c>brightness_medium</c></summary>
    public static string? BrightnessMedium => Glyph("\ue1ae");
    /// <summary>Official icon: <c>bring_your_own_ip</c></summary>
    public static string? BringYourOwnIp => Glyph("\ue016");
    /// <summary>Official icon: <c>broadcast_on_home</c></summary>
    public static string? BroadcastOnHome => Glyph("\uf8f8");
    /// <summary>Official icon: <c>broadcast_on_personal</c></summary>
    public static string? BroadcastOnPersonal => Glyph("\uf8f9");
    /// <summary>Official icon: <c>broken_image</c></summary>
    public static string? BrokenImage => Glyph("\ue3ad");
    /// <summary>Official icon: <c>browse</c></summary>
    public static string? Browse => Glyph("\ueb13");
    /// <summary>Official icon: <c>browse_activity</c></summary>
    public static string? BrowseActivity => Glyph("\uf8a5");
    /// <summary>Official icon: <c>browse_gallery</c></summary>
    public static string? BrowseGallery => Glyph("\uebd1");
    /// <summary>Official icon: <c>browser_not_supported</c></summary>
    public static string? BrowserNotSupported => Glyph("\uef47");
    /// <summary>Official icon: <c>browser_updated</c></summary>
    public static string? BrowserUpdated => Glyph("\ue7cf");
    /// <summary>Official icon: <c>brunch_dining</c></summary>
    public static string? BrunchDining => Glyph("\uea73");
    /// <summary>Official icon: <c>brush</c></summary>
    public static string? Brush => Glyph("\ue3ae");
    /// <summary>Official icon: <c>bubble</c></summary>
    public static string? Bubble => Glyph("\uef83");
    /// <summary>Official icon: <c>bubble_chart</c></summary>
    public static string? BubbleChart => Glyph("\ue6dd");
    /// <summary>Official icon: <c>bubbles</c></summary>
    public static string? Bubbles => Glyph("\uf64e");
    /// <summary>Official icon: <c>bucket_check</c></summary>
    public static string? BucketCheck => Glyph("\uef2a");
    /// <summary>Official icon: <c>bug_report</c></summary>
    public static string? BugReport => Glyph("\ue868");
    /// <summary>Official icon: <c>build</c></summary>
    public static string? Build => Glyph("\uf8cd");
    /// <summary>Official icon: <c>build_circle</c></summary>
    public static string? BuildCircle => Glyph("\uef48");
    /// <summary>Official icon: <c>bullet_chart</c></summary>
    public static string? BulletChart => Glyph("\U000FFEC7");
    /// <summary>Official icon: <c>bungalow</c></summary>
    public static string? Bungalow => Glyph("\ue591");
    /// <summary>Official icon: <c>burst_mode</c></summary>
    public static string? BurstMode => Glyph("\ue43c");
    /// <summary>Official icon: <c>bus_alert</c></summary>
    public static string? BusAlert => Glyph("\ue98f");
    /// <summary>Official icon: <c>bus_map_pin</c></summary>
    public static string? BusMapPin => Glyph("\U000FFFA2");
    /// <summary>Official icon: <c>bus_railway</c></summary>
    public static string? BusRailway => Glyph("\uf36b");
    /// <summary>Official icon: <c>business</c></summary>
    public static string? Business => Glyph("\ue7ee");
    /// <summary>Official icon: <c>business_center</c></summary>
    public static string? BusinessCenter => Glyph("\ueb3f");
    /// <summary>Official icon: <c>business_chip</c></summary>
    public static string? BusinessChip => Glyph("\uf84c");
    /// <summary>Official icon: <c>business_messages</c></summary>
    public static string? BusinessMessages => Glyph("\uef84");
    /// <summary>Official icon: <c>buttons_alt</c></summary>
    public static string? ButtonsAlt => Glyph("\ue72f");
    /// <summary>Official icon: <c>cabin</c></summary>
    public static string? Cabin => Glyph("\ue589");
    /// <summary>Official icon: <c>cable</c></summary>
    public static string? Cable => Glyph("\uefe6");
    /// <summary>Official icon: <c>cable_car</c></summary>
    public static string? CableCar => Glyph("\uf479");
    /// <summary>Official icon: <c>cached</c></summary>
    public static string? Cached => Glyph("\ue86a");
    /// <summary>Official icon: <c>cadence</c></summary>
    public static string? Cadence => Glyph("\uf4b4");
    /// <summary>Official icon: <c>cake</c></summary>
    public static string? Cake => Glyph("\ue7e9");
    /// <summary>Official icon: <c>cake_add</c></summary>
    public static string? CakeAdd => Glyph("\uf85b");
    /// <summary>Official icon: <c>calculate</c></summary>
    public static string? Calculate => Glyph("\uea5f");
    /// <summary>Official icon: <c>calendar_add_on</c></summary>
    public static string? CalendarAddOn => Glyph("\uef85");
    /// <summary>Official icon: <c>calendar_apps_script</c></summary>
    public static string? CalendarAppsScript => Glyph("\uf0bb");
    /// <summary>Official icon: <c>calendar_check</c></summary>
    public static string? CalendarCheck => Glyph("\uf243");
    /// <summary>Official icon: <c>calendar_clock</c></summary>
    public static string? CalendarClock => Glyph("\uf540");
    /// <summary>Official icon: <c>calendar_lock</c></summary>
    public static string? CalendarLock => Glyph("\uf242");
    /// <summary>Official icon: <c>calendar_meal</c></summary>
    public static string? CalendarMeal => Glyph("\uf296");
    /// <summary>Official icon: <c>calendar_meal_2</c></summary>
    public static string? CalendarMeal2 => Glyph("\uf240");
    /// <summary>Official icon: <c>calendar_month</c></summary>
    public static string? CalendarMonth => Glyph("\uebcc");
    /// <summary>Official icon: <c>calendar_today</c></summary>
    public static string? CalendarToday => Glyph("\ue935");
    /// <summary>Official icon: <c>calendar_view_day</c></summary>
    public static string? CalendarViewDay => Glyph("\ue936");
    /// <summary>Official icon: <c>calendar_view_month</c></summary>
    public static string? CalendarViewMonth => Glyph("\uefe7");
    /// <summary>Official icon: <c>calendar_view_week</c></summary>
    public static string? CalendarViewWeek => Glyph("\uefe8");
    /// <summary>Official icon: <c>call</c></summary>
    public static string? Call => Glyph("\uf0d4");
    /// <summary>Official icon: <c>call_end</c></summary>
    public static string? CallEnd => Glyph("\uf0bc");
    /// <summary>Official icon: <c>call_end_alt</c></summary>
    public static string? CallEndAlt => Glyph("\uf0bc");
    /// <summary>Official icon: <c>call_log</c></summary>
    public static string? CallLog => Glyph("\ue08e");
    /// <summary>Official icon: <c>call_made</c></summary>
    public static string? CallMade => Glyph("\ue0b2");
    /// <summary>Official icon: <c>call_merge</c></summary>
    public static string? CallMerge => Glyph("\ue0b3");
    /// <summary>Official icon: <c>call_missed</c></summary>
    public static string? CallMissed => Glyph("\ue0b4");
    /// <summary>Official icon: <c>call_missed_outgoing</c></summary>
    public static string? CallMissedOutgoing => Glyph("\ue0e4");
    /// <summary>Official icon: <c>call_quality</c></summary>
    public static string? CallQuality => Glyph("\uf652");
    /// <summary>Official icon: <c>call_received</c></summary>
    public static string? CallReceived => Glyph("\ue0b5");
    /// <summary>Official icon: <c>call_split</c></summary>
    public static string? CallSplit => Glyph("\ue0b6");
    /// <summary>Official icon: <c>call_to_action</c></summary>
    public static string? CallToAction => Glyph("\ue06c");
    /// <summary>Official icon: <c>camera</c></summary>
    public static string? Camera => Glyph("\ue3af");
    /// <summary>Official icon: <c>camera_alt</c></summary>
    public static string? CameraAlt => Glyph("\ue412");
    /// <summary>Official icon: <c>camera_enhance</c></summary>
    public static string? CameraEnhance => Glyph("\ue8fc");
    /// <summary>Official icon: <c>camera_front</c></summary>
    public static string? CameraFront => Glyph("\uf2c9");
    /// <summary>Official icon: <c>camera_indoor</c></summary>
    public static string? CameraIndoor => Glyph("\uefe9");
    /// <summary>Official icon: <c>camera_outdoor</c></summary>
    public static string? CameraOutdoor => Glyph("\uefea");
    /// <summary>Official icon: <c>camera_rear</c></summary>
    public static string? CameraRear => Glyph("\uf2c8");
    /// <summary>Official icon: <c>camera_roll</c></summary>
    public static string? CameraRoll => Glyph("\ue3b3");
    /// <summary>Official icon: <c>camera_video</c></summary>
    public static string? CameraVideo => Glyph("\uf7a6");
    /// <summary>Official icon: <c>cameraswitch</c></summary>
    public static string? Cameraswitch => Glyph("\uefeb");
    /// <summary>Official icon: <c>campaign</c></summary>
    public static string? Campaign => Glyph("\uef49");
    /// <summary>Official icon: <c>camping</c></summary>
    public static string? Camping => Glyph("\uf8a2");
    /// <summary>Official icon: <c>cancel</c></summary>
    public static string? Cancel => Glyph("\ue888");
    /// <summary>Official icon: <c>cancel_presentation</c></summary>
    public static string? CancelPresentation => Glyph("\ue0e9");
    /// <summary>Official icon: <c>cancel_schedule_send</c></summary>
    public static string? CancelScheduleSend => Glyph("\uea39");
    /// <summary>Official icon: <c>candle</c></summary>
    public static string? Candle => Glyph("\uf588");
    /// <summary>Official icon: <c>candlestick_chart</c></summary>
    public static string? CandlestickChart => Glyph("\uead4");
    /// <summary>Official icon: <c>cannabis</c></summary>
    public static string? Cannabis => Glyph("\uf2f3");
    /// <summary>Official icon: <c>captive_portal</c></summary>
    public static string? CaptivePortal => Glyph("\uf728");
    /// <summary>Official icon: <c>capture</c></summary>
    public static string? Capture => Glyph("\uf727");
    /// <summary>Official icon: <c>car_crash</c></summary>
    public static string? CarCrash => Glyph("\uebf2");
    /// <summary>Official icon: <c>car_defrost_left</c></summary>
    public static string? CarDefrostLeft => Glyph("\uf344");
    /// <summary>Official icon: <c>car_defrost_low_left</c></summary>
    public static string? CarDefrostLowLeft => Glyph("\uf343");
    /// <summary>Official icon: <c>car_defrost_low_right</c></summary>
    public static string? CarDefrostLowRight => Glyph("\uf342");
    /// <summary>Official icon: <c>car_defrost_mid_left</c></summary>
    public static string? CarDefrostMidLeft => Glyph("\uf278");
    /// <summary>Official icon: <c>car_defrost_mid_low_left</c></summary>
    public static string? CarDefrostMidLowLeft => Glyph("\uf341");
    /// <summary>Official icon: <c>car_defrost_mid_low_right</c></summary>
    public static string? CarDefrostMidLowRight => Glyph("\uf277");
    /// <summary>Official icon: <c>car_defrost_mid_right</c></summary>
    public static string? CarDefrostMidRight => Glyph("\uf340");
    /// <summary>Official icon: <c>car_defrost_right</c></summary>
    public static string? CarDefrostRight => Glyph("\uf33f");
    /// <summary>Official icon: <c>car_fan_low_left</c></summary>
    public static string? CarFanLowLeft => Glyph("\uf33e");
    /// <summary>Official icon: <c>car_fan_low_mid_left</c></summary>
    public static string? CarFanLowMidLeft => Glyph("\uf33d");
    /// <summary>Official icon: <c>car_fan_low_right</c></summary>
    public static string? CarFanLowRight => Glyph("\uf33c");
    /// <summary>Official icon: <c>car_fan_mid_left</c></summary>
    public static string? CarFanMidLeft => Glyph("\uf33b");
    /// <summary>Official icon: <c>car_fan_mid_low_right</c></summary>
    public static string? CarFanMidLowRight => Glyph("\uf33a");
    /// <summary>Official icon: <c>car_fan_mid_right</c></summary>
    public static string? CarFanMidRight => Glyph("\uf339");
    /// <summary>Official icon: <c>car_fan_recirculate</c></summary>
    public static string? CarFanRecirculate => Glyph("\uf338");
    /// <summary>Official icon: <c>car_fan_recirculate_2</c></summary>
    public static string? CarFanRecirculate2 => Glyph("\U000FFF40");
    /// <summary>Official icon: <c>car_gear</c></summary>
    public static string? CarGear => Glyph("\uf337");
    /// <summary>Official icon: <c>car_lock</c></summary>
    public static string? CarLock => Glyph("\uf336");
    /// <summary>Official icon: <c>car_mirror_heat</c></summary>
    public static string? CarMirrorHeat => Glyph("\uf335");
    /// <summary>Official icon: <c>car_rental</c></summary>
    public static string? CarRental => Glyph("\uea55");
    /// <summary>Official icon: <c>car_repair</c></summary>
    public static string? CarRepair => Glyph("\uea56");
    /// <summary>Official icon: <c>car_seat_off</c></summary>
    public static string? CarSeatOff => Glyph("\U000FFEBE");
    /// <summary>Official icon: <c>car_tag</c></summary>
    public static string? CarTag => Glyph("\uf4e3");
    /// <summary>Official icon: <c>card_giftcard</c></summary>
    public static string? CardGiftcard => Glyph("\ue8f6");
    /// <summary>Official icon: <c>card_membership</c></summary>
    public static string? CardMembership => Glyph("\ue8f7");
    /// <summary>Official icon: <c>card_travel</c></summary>
    public static string? CardTravel => Glyph("\ue8f8");
    /// <summary>Official icon: <c>cardio_load</c></summary>
    public static string? CardioLoad => Glyph("\uf4b9");
    /// <summary>Official icon: <c>cardiology</c></summary>
    public static string? Cardiology => Glyph("\ue09c");
    /// <summary>Official icon: <c>cards</c></summary>
    public static string? Cards => Glyph("\ue991");
    /// <summary>Official icon: <c>cards_stack</c></summary>
    public static string? CardsStack => Glyph("\uf38f");
    /// <summary>Official icon: <c>cards_star</c></summary>
    public static string? CardsStar => Glyph("\uf375");
    /// <summary>Official icon: <c>carpenter</c></summary>
    public static string? Carpenter => Glyph("\uf1f8");
    /// <summary>Official icon: <c>carry_on_bag</c></summary>
    public static string? CarryOnBag => Glyph("\ueb08");
    /// <summary>Official icon: <c>carry_on_bag_checked</c></summary>
    public static string? CarryOnBagChecked => Glyph("\ueb0b");
    /// <summary>Official icon: <c>carry_on_bag_inactive</c></summary>
    public static string? CarryOnBagInactive => Glyph("\ueb0a");
    /// <summary>Official icon: <c>carry_on_bag_question</c></summary>
    public static string? CarryOnBagQuestion => Glyph("\ueb09");
    /// <summary>Official icon: <c>cases</c></summary>
    public static string? Cases => Glyph("\ue992");
    /// <summary>Official icon: <c>casino</c></summary>
    public static string? Casino => Glyph("\ueb40");
    /// <summary>Official icon: <c>cast</c></summary>
    public static string? Cast => Glyph("\ue307");
    /// <summary>Official icon: <c>cast_connected</c></summary>
    public static string? CastConnected => Glyph("\ue308");
    /// <summary>Official icon: <c>cast_for_education</c></summary>
    public static string? CastForEducation => Glyph("\uefec");
    /// <summary>Official icon: <c>cast_pause</c></summary>
    public static string? CastPause => Glyph("\uf5f0");
    /// <summary>Official icon: <c>cast_warning</c></summary>
    public static string? CastWarning => Glyph("\uf5ef");
    /// <summary>Official icon: <c>castle</c></summary>
    public static string? Castle => Glyph("\ueab1");
    /// <summary>Official icon: <c>category</c></summary>
    public static string? Category => Glyph("\ue72c");
    /// <summary>Official icon: <c>category_search</c></summary>
    public static string? CategorySearch => Glyph("\uf437");
    /// <summary>Official icon: <c>celebration</c></summary>
    public static string? Celebration => Glyph("\uea65");
    /// <summary>Official icon: <c>cell_merge</c></summary>
    public static string? CellMerge => Glyph("\uf82e");
    /// <summary>Official icon: <c>cell_tower</c></summary>
    public static string? CellTower => Glyph("\uebba");
    /// <summary>Official icon: <c>cell_wifi</c></summary>
    public static string? CellWifi => Glyph("\ue0ec");
    /// <summary>Official icon: <c>center_focus_strong</c></summary>
    public static string? CenterFocusStrong => Glyph("\ue3b4");
    /// <summary>Official icon: <c>center_focus_weak</c></summary>
    public static string? CenterFocusWeak => Glyph("\ue3b5");
    /// <summary>Official icon: <c>chair</c></summary>
    public static string? Chair => Glyph("\uefed");
    /// <summary>Official icon: <c>chair_alt</c></summary>
    public static string? ChairAlt => Glyph("\uefee");
    /// <summary>Official icon: <c>chair_counter</c></summary>
    public static string? ChairCounter => Glyph("\uf29f");
    /// <summary>Official icon: <c>chair_fireplace</c></summary>
    public static string? ChairFireplace => Glyph("\uf29e");
    /// <summary>Official icon: <c>chair_umbrella</c></summary>
    public static string? ChairUmbrella => Glyph("\uf29d");
    /// <summary>Official icon: <c>chalet</c></summary>
    public static string? Chalet => Glyph("\ue585");
    /// <summary>Official icon: <c>change_circle</c></summary>
    public static string? ChangeCircle => Glyph("\ue2e7");
    /// <summary>Official icon: <c>change_history</c></summary>
    public static string? ChangeHistory => Glyph("\ue86b");
    /// <summary>Official icon: <c>charger</c></summary>
    public static string? Charger => Glyph("\ue2ae");
    /// <summary>Official icon: <c>charging_station</c></summary>
    public static string? ChargingStation => Glyph("\uf2e3");
    /// <summary>Official icon: <c>chart_data</c></summary>
    public static string? ChartData => Glyph("\ue473");
    /// <summary>Official icon: <c>chat</c></summary>
    public static string? Chat => Glyph("\ue0c9");
    /// <summary>Official icon: <c>chat_add_on</c></summary>
    public static string? ChatAddOn => Glyph("\uf0f3");
    /// <summary>Official icon: <c>chat_apps_script</c></summary>
    public static string? ChatAppsScript => Glyph("\uf0bd");
    /// <summary>Official icon: <c>chat_bubble</c></summary>
    public static string? ChatBubble => Glyph("\ue0cb");
    /// <summary>Official icon: <c>chat_bubble_off</c></summary>
    public static string? ChatBubbleOff => Glyph("\U000FFFBB");
    /// <summary>Official icon: <c>chat_bubble_outline</c></summary>
    public static string? ChatBubbleOutline => Glyph("\ue0cb");
    /// <summary>Official icon: <c>chat_dashed</c></summary>
    public static string? ChatDashed => Glyph("\ueeed");
    /// <summary>Official icon: <c>chat_error</c></summary>
    public static string? ChatError => Glyph("\uf7ac");
    /// <summary>Official icon: <c>chat_info</c></summary>
    public static string? ChatInfo => Glyph("\uf52b");
    /// <summary>Official icon: <c>chat_paste_go</c></summary>
    public static string? ChatPasteGo => Glyph("\uf6bd");
    /// <summary>Official icon: <c>chat_paste_go_2</c></summary>
    public static string? ChatPasteGo2 => Glyph("\uf3cb");
    /// <summary>Official icon: <c>check</c></summary>
    public static string? Check => Glyph("\ue668");
    /// <summary>Official icon: <c>check_alert</c></summary>
    public static string? CheckAlert => Glyph("\U000FFF85");
    /// <summary>Official icon: <c>check_box</c></summary>
    public static string? CheckBox => Glyph("\ue9de");
    /// <summary>Official icon: <c>check_box_outline_blank</c></summary>
    public static string? CheckBoxOutlineBlank => Glyph("\ue835");
    /// <summary>Official icon: <c>check_circle</c></summary>
    public static string? CheckCircle => Glyph("\uf0be");
    /// <summary>Official icon: <c>check_circle_filled</c></summary>
    public static string? CheckCircleFilled => Glyph("\uf0be");
    /// <summary>Official icon: <c>check_circle_outline</c></summary>
    public static string? CheckCircleOutline => Glyph("\uf0be");
    /// <summary>Official icon: <c>check_circle_unread</c></summary>
    public static string? CheckCircleUnread => Glyph("\uf27e");
    /// <summary>Official icon: <c>check_in_out</c></summary>
    public static string? CheckInOut => Glyph("\uf6f6");
    /// <summary>Official icon: <c>check_indeterminate_small</c></summary>
    public static string? CheckIndeterminateSmall => Glyph("\uf88a");
    /// <summary>Official icon: <c>check_small</c></summary>
    public static string? CheckSmall => Glyph("\uf88b");
    /// <summary>Official icon: <c>checkbook</c></summary>
    public static string? Checkbook => Glyph("\ue70d");
    /// <summary>Official icon: <c>checked_bag</c></summary>
    public static string? CheckedBag => Glyph("\ueb0c");
    /// <summary>Official icon: <c>checked_bag_question</c></summary>
    public static string? CheckedBagQuestion => Glyph("\ueb0d");
    /// <summary>Official icon: <c>checklist</c></summary>
    public static string? Checklist => Glyph("\ue6b1");
    /// <summary>Official icon: <c>checklist_rtl</c></summary>
    public static string? ChecklistRtl => Glyph("\ue6b3");
    /// <summary>Official icon: <c>checkroom</c></summary>
    public static string? Checkroom => Glyph("\uf19e");
    /// <summary>Official icon: <c>cheer</c></summary>
    public static string? Cheer => Glyph("\uf6a8");
    /// <summary>Official icon: <c>chef_hat</c></summary>
    public static string? ChefHat => Glyph("\uf357");
    /// <summary>Official icon: <c>chess</c></summary>
    public static string? Chess => Glyph("\uf5e7");
    /// <summary>Official icon: <c>chess_bishop</c></summary>
    public static string? ChessBishop => Glyph("\uf261");
    /// <summary>Official icon: <c>chess_bishop_2</c></summary>
    public static string? ChessBishop2 => Glyph("\uf262");
    /// <summary>Official icon: <c>chess_king</c></summary>
    public static string? ChessKing => Glyph("\uf25f");
    /// <summary>Official icon: <c>chess_king_2</c></summary>
    public static string? ChessKing2 => Glyph("\uf260");
    /// <summary>Official icon: <c>chess_knight</c></summary>
    public static string? ChessKnight => Glyph("\uf25e");
    /// <summary>Official icon: <c>chess_pawn</c></summary>
    public static string? ChessPawn => Glyph("\uf3b6");
    /// <summary>Official icon: <c>chess_pawn_2</c></summary>
    public static string? ChessPawn2 => Glyph("\uf25d");
    /// <summary>Official icon: <c>chess_queen</c></summary>
    public static string? ChessQueen => Glyph("\uf25c");
    /// <summary>Official icon: <c>chess_rook</c></summary>
    public static string? ChessRook => Glyph("\uf25b");
    /// <summary>Official icon: <c>chevron_backward</c></summary>
    public static string? ChevronBackward => Glyph("\uf46b");
    /// <summary>Official icon: <c>chevron_forward</c></summary>
    public static string? ChevronForward => Glyph("\uf46a");
    /// <summary>Official icon: <c>chevron_left</c></summary>
    public static string? ChevronLeft => Glyph("\ue5cb");
    /// <summary>Official icon: <c>chevron_line_up</c></summary>
    public static string? ChevronLineUp => Glyph("\ueec3");
    /// <summary>Official icon: <c>chevron_right</c></summary>
    public static string? ChevronRight => Glyph("\ue5cc");
    /// <summary>Official icon: <c>child_care</c></summary>
    public static string? ChildCare => Glyph("\ueb41");
    /// <summary>Official icon: <c>child_friendly</c></summary>
    public static string? ChildFriendly => Glyph("\uef80");
    /// <summary>Official icon: <c>child_hat</c></summary>
    public static string? ChildHat => Glyph("\uef30");
    /// <summary>Official icon: <c>chip_extraction</c></summary>
    public static string? ChipExtraction => Glyph("\uf821");
    /// <summary>Official icon: <c>chips</c></summary>
    public static string? Chips => Glyph("\ue993");
    /// <summary>Official icon: <c>chrome_reader_mode</c></summary>
    public static string? ChromeReaderMode => Glyph("\ue86d");
    /// <summary>Official icon: <c>chromecast_2</c></summary>
    public static string? Chromecast2 => Glyph("\uf17b");
    /// <summary>Official icon: <c>chromecast_device</c></summary>
    public static string? ChromecastDevice => Glyph("\ue83c");
    /// <summary>Official icon: <c>chronic</c></summary>
    public static string? Chronic => Glyph("\uebb2");
    /// <summary>Official icon: <c>church</c></summary>
    public static string? Church => Glyph("\ueaae");
    /// <summary>Official icon: <c>cinematic_blur</c></summary>
    public static string? CinematicBlur => Glyph("\uf853");
    /// <summary>Official icon: <c>circle</c></summary>
    public static string? Circle => Glyph("\uef4a");
    /// <summary>Official icon: <c>circle_circle</c></summary>
    public static string? CircleCircle => Glyph("\ueee1");
    /// <summary>Official icon: <c>circle_notifications</c></summary>
    public static string? CircleNotifications => Glyph("\ue994");
    /// <summary>Official icon: <c>circles</c></summary>
    public static string? Circles => Glyph("\ue7ea");
    /// <summary>Official icon: <c>circles_ext</c></summary>
    public static string? CirclesExt => Glyph("\ue7ec");
    /// <summary>Official icon: <c>clarify</c></summary>
    public static string? Clarify => Glyph("\uf0bf");
    /// <summary>Official icon: <c>class</c></summary>
    public static string? Class => Glyph("\ue86e");
    /// <summary>Official icon: <c>clean_hands</c></summary>
    public static string? CleanHands => Glyph("\uf21f");
    /// <summary>Official icon: <c>cleaning</c></summary>
    public static string? Cleaning => Glyph("\ue995");
    /// <summary>Official icon: <c>cleaning_bucket</c></summary>
    public static string? CleaningBucket => Glyph("\uf8b4");
    /// <summary>Official icon: <c>cleaning_services</c></summary>
    public static string? CleaningServices => Glyph("\uf0ff");
    /// <summary>Official icon: <c>clear</c></summary>
    public static string? Clear => Glyph("\ue5cd");
    /// <summary>Official icon: <c>clear_all</c></summary>
    public static string? ClearAll => Glyph("\ue0b8");
    /// <summary>Official icon: <c>clear_day</c></summary>
    public static string? ClearDay => Glyph("\uf157");
    /// <summary>Official icon: <c>clear_night</c></summary>
    public static string? ClearNight => Glyph("\uf159");
    /// <summary>Official icon: <c>climate_mini_split</c></summary>
    public static string? ClimateMiniSplit => Glyph("\uf8b5");
    /// <summary>Official icon: <c>clinical_notes</c></summary>
    public static string? ClinicalNotes => Glyph("\ue09e");
    /// <summary>Official icon: <c>clock_arrow_down</c></summary>
    public static string? ClockArrowDown => Glyph("\uf382");
    /// <summary>Official icon: <c>clock_arrow_up</c></summary>
    public static string? ClockArrowUp => Glyph("\uf381");
    /// <summary>Official icon: <c>clock_loader_10</c></summary>
    public static string? ClockLoader10 => Glyph("\uf726");
    /// <summary>Official icon: <c>clock_loader_20</c></summary>
    public static string? ClockLoader20 => Glyph("\uf725");
    /// <summary>Official icon: <c>clock_loader_40</c></summary>
    public static string? ClockLoader40 => Glyph("\uf724");
    /// <summary>Official icon: <c>clock_loader_60</c></summary>
    public static string? ClockLoader60 => Glyph("\uf723");
    /// <summary>Official icon: <c>clock_loader_80</c></summary>
    public static string? ClockLoader80 => Glyph("\uf722");
    /// <summary>Official icon: <c>clock_loader_90</c></summary>
    public static string? ClockLoader90 => Glyph("\uf721");
    /// <summary>Official icon: <c>close</c></summary>
    public static string? Close => Glyph("\ue5cd");
    /// <summary>Official icon: <c>close_fullscreen</c></summary>
    public static string? CloseFullscreen => Glyph("\uf1cf");
    /// <summary>Official icon: <c>close_small</c></summary>
    public static string? CloseSmall => Glyph("\uf508");
    /// <summary>Official icon: <c>closed_caption</c></summary>
    public static string? ClosedCaption => Glyph("\ue996");
    /// <summary>Official icon: <c>closed_caption_add</c></summary>
    public static string? ClosedCaptionAdd => Glyph("\uf4ae");
    /// <summary>Official icon: <c>closed_caption_disabled</c></summary>
    public static string? ClosedCaptionDisabled => Glyph("\uf1dc");
    /// <summary>Official icon: <c>closed_caption_off</c></summary>
    public static string? ClosedCaptionOff => Glyph("\ue996");
    /// <summary>Official icon: <c>cloud</c></summary>
    public static string? Cloud => Glyph("\uf15c");
    /// <summary>Official icon: <c>cloud_alert</c></summary>
    public static string? CloudAlert => Glyph("\uf3cc");
    /// <summary>Official icon: <c>cloud_circle</c></summary>
    public static string? CloudCircle => Glyph("\ue2be");
    /// <summary>Official icon: <c>cloud_done</c></summary>
    public static string? CloudDone => Glyph("\ue2bf");
    /// <summary>Official icon: <c>cloud_download</c></summary>
    public static string? CloudDownload => Glyph("\ue2c0");
    /// <summary>Official icon: <c>cloud_lock</c></summary>
    public static string? CloudLock => Glyph("\uf386");
    /// <summary>Official icon: <c>cloud_off</c></summary>
    public static string? CloudOff => Glyph("\ue2c1");
    /// <summary>Official icon: <c>cloud_queue</c></summary>
    public static string? CloudQueue => Glyph("\uf15c");
    /// <summary>Official icon: <c>cloud_sync</c></summary>
    public static string? CloudSync => Glyph("\ueb5a");
    /// <summary>Official icon: <c>cloud_upload</c></summary>
    public static string? CloudUpload => Glyph("\ue2c3");
    /// <summary>Official icon: <c>cloudy</c></summary>
    public static string? Cloudy => Glyph("\uf15c");
    /// <summary>Official icon: <c>cloudy_filled</c></summary>
    public static string? CloudyFilled => Glyph("\uf15c");
    /// <summary>Official icon: <c>cloudy_snowing</c></summary>
    public static string? CloudySnowing => Glyph("\ue810");
    /// <summary>Official icon: <c>co2</c></summary>
    public static string? Co2 => Glyph("\ue7b0");
    /// <summary>Official icon: <c>co_present</c></summary>
    public static string? CoPresent => Glyph("\ueaf0");
    /// <summary>Official icon: <c>code</c></summary>
    public static string? Code => Glyph("\ue86f");
    /// <summary>Official icon: <c>code_blocks</c></summary>
    public static string? CodeBlocks => Glyph("\uf84d");
    /// <summary>Official icon: <c>code_off</c></summary>
    public static string? CodeOff => Glyph("\ue4f3");
    /// <summary>Official icon: <c>code_xml</c></summary>
    public static string? CodeXml => Glyph("\U000FFF8B");
    /// <summary>Official icon: <c>coffee</c></summary>
    public static string? Coffee => Glyph("\uefef");
    /// <summary>Official icon: <c>coffee_maker</c></summary>
    public static string? CoffeeMaker => Glyph("\ueff0");
    /// <summary>Official icon: <c>cognition</c></summary>
    public static string? Cognition => Glyph("\ue09f");
    /// <summary>Official icon: <c>cognition_2</c></summary>
    public static string? Cognition2 => Glyph("\uf3b5");
    /// <summary>Official icon: <c>collapse_all</c></summary>
    public static string? CollapseAll => Glyph("\ue944");
    /// <summary>Official icon: <c>collapse_content</c></summary>
    public static string? CollapseContent => Glyph("\uf507");
    /// <summary>Official icon: <c>collections</c></summary>
    public static string? Collections => Glyph("\ue3d3");
    /// <summary>Official icon: <c>collections_bookmark</c></summary>
    public static string? CollectionsBookmark => Glyph("\ue431");
    /// <summary>Official icon: <c>color_lens</c></summary>
    public static string? ColorLens => Glyph("\ue40a");
    /// <summary>Official icon: <c>colorize</c></summary>
    public static string? Colorize => Glyph("\ue3b8");
    /// <summary>Official icon: <c>colors</c></summary>
    public static string? Colors => Glyph("\ue997");
    /// <summary>Official icon: <c>combine_columns</c></summary>
    public static string? CombineColumns => Glyph("\uf420");
    /// <summary>Official icon: <c>comedy_mask</c></summary>
    public static string? ComedyMask => Glyph("\uf4d6");
    /// <summary>Official icon: <c>comic_bubble</c></summary>
    public static string? ComicBubble => Glyph("\uf5dd");
    /// <summary>Official icon: <c>comment</c></summary>
    public static string? Comment => Glyph("\ue24c");
    /// <summary>Official icon: <c>comment_bank</c></summary>
    public static string? CommentBank => Glyph("\uea4e");
    /// <summary>Official icon: <c>comments_disabled</c></summary>
    public static string? CommentsDisabled => Glyph("\ue7a2");
    /// <summary>Official icon: <c>commit</c></summary>
    public static string? Commit => Glyph("\ueaf5");
    /// <summary>Official icon: <c>communication</c></summary>
    public static string? Communication => Glyph("\ue27c");
    /// <summary>Official icon: <c>communities</c></summary>
    public static string? Communities => Glyph("\ueb16");
    /// <summary>Official icon: <c>communities_filled</c></summary>
    public static string? CommunitiesFilled => Glyph("\ueb16");
    /// <summary>Official icon: <c>commute</c></summary>
    public static string? Commute => Glyph("\ue940");
    /// <summary>Official icon: <c>compare</c></summary>
    public static string? Compare => Glyph("\ue3b9");
    /// <summary>Official icon: <c>compare_arrows</c></summary>
    public static string? CompareArrows => Glyph("\ue915");
    /// <summary>Official icon: <c>compass_calibration</c></summary>
    public static string? CompassCalibration => Glyph("\ue57c");
    /// <summary>Official icon: <c>component_exchange</c></summary>
    public static string? ComponentExchange => Glyph("\uf1e7");
    /// <summary>Official icon: <c>compost</c></summary>
    public static string? Compost => Glyph("\ue761");
    /// <summary>Official icon: <c>compress</c></summary>
    public static string? Compress => Glyph("\ue94d");
    /// <summary>Official icon: <c>computer</c></summary>
    public static string? Computer => Glyph("\ue31e");
    /// <summary>Official icon: <c>computer_arrow_up</c></summary>
    public static string? ComputerArrowUp => Glyph("\uf2f7");
    /// <summary>Official icon: <c>computer_cancel</c></summary>
    public static string? ComputerCancel => Glyph("\uf2f6");
    /// <summary>Official icon: <c>computer_sound</c></summary>
    public static string? ComputerSound => Glyph("\ueeb4");
    /// <summary>Official icon: <c>concierge</c></summary>
    public static string? Concierge => Glyph("\uf561");
    /// <summary>Official icon: <c>conditions</c></summary>
    public static string? Conditions => Glyph("\ue0a0");
    /// <summary>Official icon: <c>confirmation_number</c></summary>
    public static string? ConfirmationNumber => Glyph("\ue638");
    /// <summary>Official icon: <c>congenital</c></summary>
    public static string? Congenital => Glyph("\ue0a1");
    /// <summary>Official icon: <c>connect_without_contact</c></summary>
    public static string? ConnectWithoutContact => Glyph("\uf223");
    /// <summary>Official icon: <c>connected_tv</c></summary>
    public static string? ConnectedTv => Glyph("\ue998");
    /// <summary>Official icon: <c>connecting_airports</c></summary>
    public static string? ConnectingAirports => Glyph("\ue7c9");
    /// <summary>Official icon: <c>construction</c></summary>
    public static string? Construction => Glyph("\uea3c");
    /// <summary>Official icon: <c>contact_emergency</c></summary>
    public static string? ContactEmergency => Glyph("\uf8d1");
    /// <summary>Official icon: <c>contact_mail</c></summary>
    public static string? ContactMail => Glyph("\ue0d0");
    /// <summary>Official icon: <c>contact_page</c></summary>
    public static string? ContactPage => Glyph("\uf22e");
    /// <summary>Official icon: <c>contact_phone</c></summary>
    public static string? ContactPhone => Glyph("\uf0c0");
    /// <summary>Official icon: <c>contact_phone_filled</c></summary>
    public static string? ContactPhoneFilled => Glyph("\uf0c0");
    /// <summary>Official icon: <c>contact_support</c></summary>
    public static string? ContactSupport => Glyph("\ue94c");
    /// <summary>Official icon: <c>contactless</c></summary>
    public static string? Contactless => Glyph("\uea71");
    /// <summary>Official icon: <c>contactless_off</c></summary>
    public static string? ContactlessOff => Glyph("\uf858");
    /// <summary>Official icon: <c>contacts</c></summary>
    public static string? Contacts => Glyph("\ue0ba");
    /// <summary>Official icon: <c>contacts_product</c></summary>
    public static string? ContactsProduct => Glyph("\ue999");
    /// <summary>Official icon: <c>content_copy</c></summary>
    public static string? ContentCopy => Glyph("\ue14d");
    /// <summary>Official icon: <c>content_cut</c></summary>
    public static string? ContentCut => Glyph("\ue14e");
    /// <summary>Official icon: <c>content_paste</c></summary>
    public static string? ContentPaste => Glyph("\ue14f");
    /// <summary>Official icon: <c>content_paste_go</c></summary>
    public static string? ContentPasteGo => Glyph("\uea8e");
    /// <summary>Official icon: <c>content_paste_off</c></summary>
    public static string? ContentPasteOff => Glyph("\ue4f8");
    /// <summary>Official icon: <c>content_paste_search</c></summary>
    public static string? ContentPasteSearch => Glyph("\uea9b");
    /// <summary>Official icon: <c>contextual_token</c></summary>
    public static string? ContextualToken => Glyph("\uf486");
    /// <summary>Official icon: <c>contextual_token_add</c></summary>
    public static string? ContextualTokenAdd => Glyph("\uf485");
    /// <summary>Official icon: <c>contract</c></summary>
    public static string? Contract => Glyph("\uf5a0");
    /// <summary>Official icon: <c>contract_delete</c></summary>
    public static string? ContractDelete => Glyph("\uf5a2");
    /// <summary>Official icon: <c>contract_edit</c></summary>
    public static string? ContractEdit => Glyph("\uf5a1");
    /// <summary>Official icon: <c>contrast</c></summary>
    public static string? Contrast => Glyph("\ueb37");
    /// <summary>Official icon: <c>contrast_circle</c></summary>
    public static string? ContrastCircle => Glyph("\uf49f");
    /// <summary>Official icon: <c>contrast_rtl_off</c></summary>
    public static string? ContrastRtlOff => Glyph("\uec72");
    /// <summary>Official icon: <c>contrast_square</c></summary>
    public static string? ContrastSquare => Glyph("\uf4a0");
    /// <summary>Official icon: <c>control_camera</c></summary>
    public static string? ControlCamera => Glyph("\ue074");
    /// <summary>Official icon: <c>control_point</c></summary>
    public static string? ControlPoint => Glyph("\ue990");
    /// <summary>Official icon: <c>control_point_duplicate</c></summary>
    public static string? ControlPointDuplicate => Glyph("\ue3bb");
    /// <summary>Official icon: <c>controller_gen</c></summary>
    public static string? ControllerGen => Glyph("\ue83d");
    /// <summary>Official icon: <c>conversation</c></summary>
    public static string? Conversation => Glyph("\uef2f");
    /// <summary>Official icon: <c>conversion_path</c></summary>
    public static string? ConversionPath => Glyph("\uf0c1");
    /// <summary>Official icon: <c>conversion_path_off</c></summary>
    public static string? ConversionPathOff => Glyph("\uf7b4");
    /// <summary>Official icon: <c>convert_to_text</c></summary>
    public static string? ConvertToText => Glyph("\uf41f");
    /// <summary>Official icon: <c>conveyor_belt</c></summary>
    public static string? ConveyorBelt => Glyph("\uf867");
    /// <summary>Official icon: <c>cookie</c></summary>
    public static string? Cookie => Glyph("\ueaac");
    /// <summary>Official icon: <c>cookie_off</c></summary>
    public static string? CookieOff => Glyph("\uf79a");
    /// <summary>Official icon: <c>cooking</c></summary>
    public static string? Cooking => Glyph("\ue2b6");
    /// <summary>Official icon: <c>cool_to_dry</c></summary>
    public static string? CoolToDry => Glyph("\ue276");
    /// <summary>Official icon: <c>copy_all</c></summary>
    public static string? CopyAll => Glyph("\ue2ec");
    /// <summary>Official icon: <c>copyright</c></summary>
    public static string? Copyright => Glyph("\ue90c");
    /// <summary>Official icon: <c>coronavirus</c></summary>
    public static string? Coronavirus => Glyph("\uf221");
    /// <summary>Official icon: <c>corporate_fare</c></summary>
    public static string? CorporateFare => Glyph("\uf1d0");
    /// <summary>Official icon: <c>cottage</c></summary>
    public static string? Cottage => Glyph("\ue587");
    /// <summary>Official icon: <c>counter_0</c></summary>
    public static string? Counter0 => Glyph("\uf785");
    /// <summary>Official icon: <c>counter_1</c></summary>
    public static string? Counter1 => Glyph("\uf784");
    /// <summary>Official icon: <c>counter_2</c></summary>
    public static string? Counter2 => Glyph("\uf783");
    /// <summary>Official icon: <c>counter_3</c></summary>
    public static string? Counter3 => Glyph("\uf782");
    /// <summary>Official icon: <c>counter_4</c></summary>
    public static string? Counter4 => Glyph("\uf781");
    /// <summary>Official icon: <c>counter_5</c></summary>
    public static string? Counter5 => Glyph("\uf780");
    /// <summary>Official icon: <c>counter_6</c></summary>
    public static string? Counter6 => Glyph("\uf77f");
    /// <summary>Official icon: <c>counter_7</c></summary>
    public static string? Counter7 => Glyph("\uf77e");
    /// <summary>Official icon: <c>counter_8</c></summary>
    public static string? Counter8 => Glyph("\uf77d");
    /// <summary>Official icon: <c>counter_9</c></summary>
    public static string? Counter9 => Glyph("\uf77c");
    /// <summary>Official icon: <c>countertops</c></summary>
    public static string? Countertops => Glyph("\uf1f7");
    /// <summary>Official icon: <c>create</c></summary>
    public static string? Create => Glyph("\uf097");
    /// <summary>Official icon: <c>create_new_folder</c></summary>
    public static string? CreateNewFolder => Glyph("\ue2cc");
    /// <summary>Official icon: <c>credit_card</c></summary>
    public static string? CreditCard => Glyph("\ue8a1");
    /// <summary>Official icon: <c>credit_card_clock</c></summary>
    public static string? CreditCardClock => Glyph("\uf438");
    /// <summary>Official icon: <c>credit_card_gear</c></summary>
    public static string? CreditCardGear => Glyph("\uf52d");
    /// <summary>Official icon: <c>credit_card_heart</c></summary>
    public static string? CreditCardHeart => Glyph("\uf52c");
    /// <summary>Official icon: <c>credit_card_off</c></summary>
    public static string? CreditCardOff => Glyph("\ue4f4");
    /// <summary>Official icon: <c>credit_score</c></summary>
    public static string? CreditScore => Glyph("\ueff1");
    /// <summary>Official icon: <c>crib</c></summary>
    public static string? Crib => Glyph("\ue588");
    /// <summary>Official icon: <c>crisis_alert</c></summary>
    public static string? CrisisAlert => Glyph("\uebe9");
    /// <summary>Official icon: <c>crop</c></summary>
    public static string? Crop => Glyph("\ue3be");
    /// <summary>Official icon: <c>crop_16_9</c></summary>
    public static string? Crop169 => Glyph("\ue3bc");
    /// <summary>Official icon: <c>crop_21_9</c></summary>
    public static string? Crop219 => Glyph("\U000FFF0A");
    /// <summary>Official icon: <c>crop_2_3</c></summary>
    public static string? Crop23 => Glyph("\U000FFF0B");
    /// <summary>Official icon: <c>crop_3_2</c></summary>
    public static string? Crop32 => Glyph("\ue3bd");
    /// <summary>Official icon: <c>crop_5_4</c></summary>
    public static string? Crop54 => Glyph("\ue3bf");
    /// <summary>Official icon: <c>crop_7_5</c></summary>
    public static string? Crop75 => Glyph("\ue3c0");
    /// <summary>Official icon: <c>crop_9_16</c></summary>
    public static string? Crop916 => Glyph("\uf549");
    /// <summary>Official icon: <c>crop_din</c></summary>
    public static string? CropDin => Glyph("\ue3c6");
    /// <summary>Official icon: <c>crop_free</c></summary>
    public static string? CropFree => Glyph("\ue3c2");
    /// <summary>Official icon: <c>crop_landscape</c></summary>
    public static string? CropLandscape => Glyph("\ue3c3");
    /// <summary>Official icon: <c>crop_original</c></summary>
    public static string? CropOriginal => Glyph("\ue3f4");
    /// <summary>Official icon: <c>crop_portrait</c></summary>
    public static string? CropPortrait => Glyph("\ue3c5");
    /// <summary>Official icon: <c>crop_rotate</c></summary>
    public static string? CropRotate => Glyph("\ue437");
    /// <summary>Official icon: <c>crop_square</c></summary>
    public static string? CropSquare => Glyph("\ue3c6");
    /// <summary>Official icon: <c>crossword</c></summary>
    public static string? Crossword => Glyph("\uf5e5");
    /// <summary>Official icon: <c>crowdsource</c></summary>
    public static string? Crowdsource => Glyph("\ueb18");
    /// <summary>Official icon: <c>crown</c></summary>
    public static string? Crown => Glyph("\uecb3");
    /// <summary>Official icon: <c>cruelty_free</c></summary>
    public static string? CrueltyFree => Glyph("\ue799");
    /// <summary>Official icon: <c>css</c></summary>
    public static string? Css => Glyph("\ueb93");
    /// <summary>Official icon: <c>csv</c></summary>
    public static string? Csv => Glyph("\ue6cf");
    /// <summary>Official icon: <c>currency_bitcoin</c></summary>
    public static string? CurrencyBitcoin => Glyph("\uebc5");
    /// <summary>Official icon: <c>currency_exchange</c></summary>
    public static string? CurrencyExchange => Glyph("\ueb70");
    /// <summary>Official icon: <c>currency_franc</c></summary>
    public static string? CurrencyFranc => Glyph("\ueafa");
    /// <summary>Official icon: <c>currency_lira</c></summary>
    public static string? CurrencyLira => Glyph("\ueaef");
    /// <summary>Official icon: <c>currency_pound</c></summary>
    public static string? CurrencyPound => Glyph("\ueaf1");
    /// <summary>Official icon: <c>currency_ruble</c></summary>
    public static string? CurrencyRuble => Glyph("\ueaec");
    /// <summary>Official icon: <c>currency_rupee</c></summary>
    public static string? CurrencyRupee => Glyph("\ueaf7");
    /// <summary>Official icon: <c>currency_rupee_circle</c></summary>
    public static string? CurrencyRupeeCircle => Glyph("\uf460");
    /// <summary>Official icon: <c>currency_yen</c></summary>
    public static string? CurrencyYen => Glyph("\ueafb");
    /// <summary>Official icon: <c>currency_yuan</c></summary>
    public static string? CurrencyYuan => Glyph("\ueaf9");
    /// <summary>Official icon: <c>curtains</c></summary>
    public static string? Curtains => Glyph("\uec1e");
    /// <summary>Official icon: <c>curtains_closed</c></summary>
    public static string? CurtainsClosed => Glyph("\uec1d");
    /// <summary>Official icon: <c>custom_typography</c></summary>
    public static string? CustomTypography => Glyph("\ue732");
    /// <summary>Official icon: <c>cut</c></summary>
    public static string? Cut => Glyph("\uf08b");
    /// <summary>Official icon: <c>cycle</c></summary>
    public static string? Cycle => Glyph("\uf854");
    /// <summary>Official icon: <c>cyclone</c></summary>
    public static string? Cyclone => Glyph("\uebd5");
    /// <summary>Official icon: <c>dangerous</c></summary>
    public static string? Dangerous => Glyph("\ue99a");
    /// <summary>Official icon: <c>dark_mode</c></summary>
    public static string? DarkMode => Glyph("\ue51c");
    /// <summary>Official icon: <c>dashboard</c></summary>
    public static string? Dashboard => Glyph("\ue871");
    /// <summary>Official icon: <c>dashboard_2</c></summary>
    public static string? Dashboard2 => Glyph("\uf3ea");
    /// <summary>Official icon: <c>dashboard_2_add</c></summary>
    public static string? Dashboard2Add => Glyph("\U000FFEE9");
    /// <summary>Official icon: <c>dashboard_2_edit</c></summary>
    public static string? Dashboard2Edit => Glyph("\U000FFFD7");
    /// <summary>Official icon: <c>dashboard_2_gear</c></summary>
    public static string? Dashboard2Gear => Glyph("\U000FFFD6");
    /// <summary>Official icon: <c>dashboard_customize</c></summary>
    public static string? DashboardCustomize => Glyph("\ue99b");
    /// <summary>Official icon: <c>data_alert</c></summary>
    public static string? DataAlert => Glyph("\uf7f6");
    /// <summary>Official icon: <c>data_array</c></summary>
    public static string? DataArray => Glyph("\uead1");
    /// <summary>Official icon: <c>data_check</c></summary>
    public static string? DataCheck => Glyph("\uf7f2");
    /// <summary>Official icon: <c>data_exploration</c></summary>
    public static string? DataExploration => Glyph("\ue76f");
    /// <summary>Official icon: <c>data_info_alert</c></summary>
    public static string? DataInfoAlert => Glyph("\uf7f5");
    /// <summary>Official icon: <c>data_loss_prevention</c></summary>
    public static string? DataLossPrevention => Glyph("\ue2dc");
    /// <summary>Official icon: <c>data_object</c></summary>
    public static string? DataObject => Glyph("\uead3");
    /// <summary>Official icon: <c>data_saver_off</c></summary>
    public static string? DataSaverOff => Glyph("\ueff2");
    /// <summary>Official icon: <c>data_saver_on</c></summary>
    public static string? DataSaverOn => Glyph("\ueff3");
    /// <summary>Official icon: <c>data_table</c></summary>
    public static string? DataTable => Glyph("\ue99c");
    /// <summary>Official icon: <c>data_thresholding</c></summary>
    public static string? DataThresholding => Glyph("\ueb9f");
    /// <summary>Official icon: <c>data_usage</c></summary>
    public static string? DataUsage => Glyph("\ueff2");
    /// <summary>Official icon: <c>database</c></summary>
    public static string? Database => Glyph("\uf20e");
    /// <summary>Official icon: <c>database_off</c></summary>
    public static string? DatabaseOff => Glyph("\uf414");
    /// <summary>Official icon: <c>database_search</c></summary>
    public static string? DatabaseSearch => Glyph("\uf38e");
    /// <summary>Official icon: <c>database_upload</c></summary>
    public static string? DatabaseUpload => Glyph("\uf3dc");
    /// <summary>Official icon: <c>dataset</c></summary>
    public static string? Dataset => Glyph("\uf8ee");
    /// <summary>Official icon: <c>dataset_linked</c></summary>
    public static string? DatasetLinked => Glyph("\uf8ef");
    /// <summary>Official icon: <c>date_range</c></summary>
    public static string? DateRange => Glyph("\ue916");
    /// <summary>Official icon: <c>deblur</c></summary>
    public static string? Deblur => Glyph("\ueb77");
    /// <summary>Official icon: <c>deceased</c></summary>
    public static string? Deceased => Glyph("\ue0a5");
    /// <summary>Official icon: <c>decimal_decrease</c></summary>
    public static string? DecimalDecrease => Glyph("\uf82d");
    /// <summary>Official icon: <c>decimal_increase</c></summary>
    public static string? DecimalIncrease => Glyph("\uf82c");
    /// <summary>Official icon: <c>deck</c></summary>
    public static string? Deck => Glyph("\uea42");
    /// <summary>Official icon: <c>dehaze</c></summary>
    public static string? Dehaze => Glyph("\ue3c7");
    /// <summary>Official icon: <c>delete</c></summary>
    public static string? Delete => Glyph("\ue92e");
    /// <summary>Official icon: <c>delete_forever</c></summary>
    public static string? DeleteForever => Glyph("\ue92b");
    /// <summary>Official icon: <c>delete_history</c></summary>
    public static string? DeleteHistory => Glyph("\uf518");
    /// <summary>Official icon: <c>delete_outline</c></summary>
    public static string? DeleteOutline => Glyph("\ue92e");
    /// <summary>Official icon: <c>delete_sweep</c></summary>
    public static string? DeleteSweep => Glyph("\ue16c");
    /// <summary>Official icon: <c>delivery_dining</c></summary>
    public static string? DeliveryDining => Glyph("\ueb28");
    /// <summary>Official icon: <c>delivery_truck_bolt</c></summary>
    public static string? DeliveryTruckBolt => Glyph("\uf3a2");
    /// <summary>Official icon: <c>delivery_truck_speed</c></summary>
    public static string? DeliveryTruckSpeed => Glyph("\uf3a1");
    /// <summary>Official icon: <c>demography</c></summary>
    public static string? Demography => Glyph("\ue489");
    /// <summary>Official icon: <c>density_large</c></summary>
    public static string? DensityLarge => Glyph("\ueba9");
    /// <summary>Official icon: <c>density_medium</c></summary>
    public static string? DensityMedium => Glyph("\ueb9e");
    /// <summary>Official icon: <c>density_small</c></summary>
    public static string? DensitySmall => Glyph("\ueba8");
    /// <summary>Official icon: <c>dentistry</c></summary>
    public static string? Dentistry => Glyph("\ue0a6");
    /// <summary>Official icon: <c>departure_board</c></summary>
    public static string? DepartureBoard => Glyph("\ue576");
    /// <summary>Official icon: <c>deployed_code</c></summary>
    public static string? DeployedCode => Glyph("\uf720");
    /// <summary>Official icon: <c>deployed_code_account</c></summary>
    public static string? DeployedCodeAccount => Glyph("\uf51b");
    /// <summary>Official icon: <c>deployed_code_alert</c></summary>
    public static string? DeployedCodeAlert => Glyph("\uf5f2");
    /// <summary>Official icon: <c>deployed_code_history</c></summary>
    public static string? DeployedCodeHistory => Glyph("\uf5f3");
    /// <summary>Official icon: <c>deployed_code_update</c></summary>
    public static string? DeployedCodeUpdate => Glyph("\uf5f4");
    /// <summary>Official icon: <c>dermatology</c></summary>
    public static string? Dermatology => Glyph("\ue0a7");
    /// <summary>Official icon: <c>description</c></summary>
    public static string? Description => Glyph("\ue873");
    /// <summary>Official icon: <c>deselect</c></summary>
    public static string? Deselect => Glyph("\uebb6");
    /// <summary>Official icon: <c>design_services</c></summary>
    public static string? DesignServices => Glyph("\uf10a");
    /// <summary>Official icon: <c>desk</c></summary>
    public static string? Desk => Glyph("\uf8f4");
    /// <summary>Official icon: <c>deskphone</c></summary>
    public static string? Deskphone => Glyph("\uf7fa");
    /// <summary>Official icon: <c>desktop_access_disabled</c></summary>
    public static string? DesktopAccessDisabled => Glyph("\ue99d");
    /// <summary>Official icon: <c>desktop_cloud</c></summary>
    public static string? DesktopCloud => Glyph("\uf3db");
    /// <summary>Official icon: <c>desktop_cloud_stack</c></summary>
    public static string? DesktopCloudStack => Glyph("\uf3be");
    /// <summary>Official icon: <c>desktop_landscape</c></summary>
    public static string? DesktopLandscape => Glyph("\uf45e");
    /// <summary>Official icon: <c>desktop_landscape_add</c></summary>
    public static string? DesktopLandscapeAdd => Glyph("\uf439");
    /// <summary>Official icon: <c>desktop_mac</c></summary>
    public static string? DesktopMac => Glyph("\ue30b");
    /// <summary>Official icon: <c>desktop_portrait</c></summary>
    public static string? DesktopPortrait => Glyph("\uf45d");
    /// <summary>Official icon: <c>desktop_windows</c></summary>
    public static string? DesktopWindows => Glyph("\ue30c");
    /// <summary>Official icon: <c>destruction</c></summary>
    public static string? Destruction => Glyph("\uf585");
    /// <summary>Official icon: <c>details</c></summary>
    public static string? Details => Glyph("\ue3c8");
    /// <summary>Official icon: <c>detection_and_zone</c></summary>
    public static string? DetectionAndZone => Glyph("\ue29f");
    /// <summary>Official icon: <c>detection_and_zone_off</c></summary>
    public static string? DetectionAndZoneOff => Glyph("\ueebf");
    /// <summary>Official icon: <c>detector</c></summary>
    public static string? Detector => Glyph("\ue282");
    /// <summary>Official icon: <c>detector_alarm</c></summary>
    public static string? DetectorAlarm => Glyph("\ue1f7");
    /// <summary>Official icon: <c>detector_battery</c></summary>
    public static string? DetectorBattery => Glyph("\ue204");
    /// <summary>Official icon: <c>detector_co</c></summary>
    public static string? DetectorCo => Glyph("\ue2af");
    /// <summary>Official icon: <c>detector_offline</c></summary>
    public static string? DetectorOffline => Glyph("\ue223");
    /// <summary>Official icon: <c>detector_smoke</c></summary>
    public static string? DetectorSmoke => Glyph("\ue285");
    /// <summary>Official icon: <c>detector_status</c></summary>
    public static string? DetectorStatus => Glyph("\ue1e8");
    /// <summary>Official icon: <c>developer_board</c></summary>
    public static string? DeveloperBoard => Glyph("\ue30d");
    /// <summary>Official icon: <c>developer_board_off</c></summary>
    public static string? DeveloperBoardOff => Glyph("\ue4ff");
    /// <summary>Official icon: <c>developer_guide</c></summary>
    public static string? DeveloperGuide => Glyph("\ue99e");
    /// <summary>Official icon: <c>developer_mode</c></summary>
    public static string? DeveloperMode => Glyph("\uf2e2");
    /// <summary>Official icon: <c>developer_mode_tv</c></summary>
    public static string? DeveloperModeTv => Glyph("\ue874");
    /// <summary>Official icon: <c>device_band</c></summary>
    public static string? DeviceBand => Glyph("\uf2f5");
    /// <summary>Official icon: <c>device_hub</c></summary>
    public static string? DeviceHub => Glyph("\ue335");
    /// <summary>Official icon: <c>device_reset</c></summary>
    public static string? DeviceReset => Glyph("\ue8b3");
    /// <summary>Official icon: <c>device_swoosh_star</c></summary>
    public static string? DeviceSwooshStar => Glyph("\U000FFEB8");
    /// <summary>Official icon: <c>device_thermostat</c></summary>
    public static string? DeviceThermostat => Glyph("\ue1ff");
    /// <summary>Official icon: <c>device_unknown</c></summary>
    public static string? DeviceUnknown => Glyph("\uf2e1");
    /// <summary>Official icon: <c>devices</c></summary>
    public static string? Devices => Glyph("\ue326");
    /// <summary>Official icon: <c>devices_fold</c></summary>
    public static string? DevicesFold => Glyph("\uebde");
    /// <summary>Official icon: <c>devices_fold_2</c></summary>
    public static string? DevicesFold2 => Glyph("\uf406");
    /// <summary>Official icon: <c>devices_off</c></summary>
    public static string? DevicesOff => Glyph("\uf7a5");
    /// <summary>Official icon: <c>devices_other</c></summary>
    public static string? DevicesOther => Glyph("\ue337");
    /// <summary>Official icon: <c>devices_wearables</c></summary>
    public static string? DevicesWearables => Glyph("\uf6ab");
    /// <summary>Official icon: <c>dew_point</c></summary>
    public static string? DewPoint => Glyph("\uf879");
    /// <summary>Official icon: <c>diagnosis</c></summary>
    public static string? Diagnosis => Glyph("\ue0a8");
    /// <summary>Official icon: <c>diagonal_line</c></summary>
    public static string? DiagonalLine => Glyph("\uf41e");
    /// <summary>Official icon: <c>dialer_sip</c></summary>
    public static string? DialerSip => Glyph("\ue0bb");
    /// <summary>Official icon: <c>dialogs</c></summary>
    public static string? Dialogs => Glyph("\ue99f");
    /// <summary>Official icon: <c>dialpad</c></summary>
    public static string? Dialpad => Glyph("\ue0bc");
    /// <summary>Official icon: <c>diamond</c></summary>
    public static string? Diamond => Glyph("\uead5");
    /// <summary>Official icon: <c>diamond_shine</c></summary>
    public static string? DiamondShine => Glyph("\uf2b2");
    /// <summary>Official icon: <c>dictionary</c></summary>
    public static string? Dictionary => Glyph("\uf539");
    /// <summary>Official icon: <c>difference</c></summary>
    public static string? Difference => Glyph("\ueb7d");
    /// <summary>Official icon: <c>digital_out_of_home</c></summary>
    public static string? DigitalOutOfHome => Glyph("\uf1de");
    /// <summary>Official icon: <c>digital_wellbeing</c></summary>
    public static string? DigitalWellbeing => Glyph("\uef86");
    /// <summary>Official icon: <c>dine_heart</c></summary>
    public static string? DineHeart => Glyph("\uf29c");
    /// <summary>Official icon: <c>dine_in</c></summary>
    public static string? DineIn => Glyph("\uf295");
    /// <summary>Official icon: <c>dine_lamp</c></summary>
    public static string? DineLamp => Glyph("\uf29b");
    /// <summary>Official icon: <c>dining</c></summary>
    public static string? Dining => Glyph("\ueff4");
    /// <summary>Official icon: <c>dinner_dining</c></summary>
    public static string? DinnerDining => Glyph("\uea57");
    /// <summary>Official icon: <c>directions</c></summary>
    public static string? Directions => Glyph("\ue52e");
    /// <summary>Official icon: <c>directions_alt</c></summary>
    public static string? DirectionsAlt => Glyph("\uf880");
    /// <summary>Official icon: <c>directions_alt_off</c></summary>
    public static string? DirectionsAltOff => Glyph("\uf881");
    /// <summary>Official icon: <c>directions_bike</c></summary>
    public static string? DirectionsBike => Glyph("\ue52f");
    /// <summary>Official icon: <c>directions_boat</c></summary>
    public static string? DirectionsBoat => Glyph("\ueff5");
    /// <summary>Official icon: <c>directions_boat_filled</c></summary>
    public static string? DirectionsBoatFilled => Glyph("\ueff5");
    /// <summary>Official icon: <c>directions_bus</c></summary>
    public static string? DirectionsBus => Glyph("\ueff6");
    /// <summary>Official icon: <c>directions_bus_filled</c></summary>
    public static string? DirectionsBusFilled => Glyph("\ueff6");
    /// <summary>Official icon: <c>directions_car</c></summary>
    public static string? DirectionsCar => Glyph("\ueff7");
    /// <summary>Official icon: <c>directions_car_filled</c></summary>
    public static string? DirectionsCarFilled => Glyph("\ueff7");
    /// <summary>Official icon: <c>directions_off</c></summary>
    public static string? DirectionsOff => Glyph("\uf10f");
    /// <summary>Official icon: <c>directions_railway</c></summary>
    public static string? DirectionsRailway => Glyph("\ueff8");
    /// <summary>Official icon: <c>directions_railway_2</c></summary>
    public static string? DirectionsRailway2 => Glyph("\uf462");
    /// <summary>Official icon: <c>directions_railway_filled</c></summary>
    public static string? DirectionsRailwayFilled => Glyph("\ueff8");
    /// <summary>Official icon: <c>directions_run</c></summary>
    public static string? DirectionsRun => Glyph("\ue566");
    /// <summary>Official icon: <c>directions_subway</c></summary>
    public static string? DirectionsSubway => Glyph("\ueffa");
    /// <summary>Official icon: <c>directions_subway_filled</c></summary>
    public static string? DirectionsSubwayFilled => Glyph("\ueffa");
    /// <summary>Official icon: <c>directions_transit</c></summary>
    public static string? DirectionsTransit => Glyph("\ueffa");
    /// <summary>Official icon: <c>directions_transit_filled</c></summary>
    public static string? DirectionsTransitFilled => Glyph("\ueffa");
    /// <summary>Official icon: <c>directions_walk</c></summary>
    public static string? DirectionsWalk => Glyph("\ue536");
    /// <summary>Official icon: <c>directory_sync</c></summary>
    public static string? DirectorySync => Glyph("\ue394");
    /// <summary>Official icon: <c>dirty_lens</c></summary>
    public static string? DirtyLens => Glyph("\uef4b");
    /// <summary>Official icon: <c>disabled_by_default</c></summary>
    public static string? DisabledByDefault => Glyph("\uf230");
    /// <summary>Official icon: <c>disabled_visible</c></summary>
    public static string? DisabledVisible => Glyph("\ue76e");
    /// <summary>Official icon: <c>disc_full</c></summary>
    public static string? DiscFull => Glyph("\ue610");
    /// <summary>Official icon: <c>discover_tune</c></summary>
    public static string? DiscoverTune => Glyph("\ue018");
    /// <summary>Official icon: <c>dishwasher</c></summary>
    public static string? Dishwasher => Glyph("\ue9a0");
    /// <summary>Official icon: <c>dishwasher_gen</c></summary>
    public static string? DishwasherGen => Glyph("\ue832");
    /// <summary>Official icon: <c>display_add</c></summary>
    public static string? DisplayAdd => Glyph("\U000FFED2");
    /// <summary>Official icon: <c>display_external_input</c></summary>
    public static string? DisplayExternalInput => Glyph("\uf7e7");
    /// <summary>Official icon: <c>display_settings</c></summary>
    public static string? DisplaySettings => Glyph("\ueb97");
    /// <summary>Official icon: <c>distance</c></summary>
    public static string? Distance => Glyph("\uf6ea");
    /// <summary>Official icon: <c>diversity_1</c></summary>
    public static string? Diversity1 => Glyph("\uf8d7");
    /// <summary>Official icon: <c>diversity_2</c></summary>
    public static string? Diversity2 => Glyph("\uf8d8");
    /// <summary>Official icon: <c>diversity_3</c></summary>
    public static string? Diversity3 => Glyph("\uf8d9");
    /// <summary>Official icon: <c>diversity_4</c></summary>
    public static string? Diversity4 => Glyph("\uf857");
    /// <summary>Official icon: <c>dns</c></summary>
    public static string? Dns => Glyph("\ue875");
    /// <summary>Official icon: <c>do_disturb</c></summary>
    public static string? DoDisturb => Glyph("\uf08c");
    /// <summary>Official icon: <c>do_disturb_alt</c></summary>
    public static string? DoDisturbAlt => Glyph("\uf08d");
    /// <summary>Official icon: <c>do_disturb_off</c></summary>
    public static string? DoDisturbOff => Glyph("\uf08e");
    /// <summary>Official icon: <c>do_disturb_on</c></summary>
    public static string? DoDisturbOn => Glyph("\uf08f");
    /// <summary>Official icon: <c>do_not_disturb</c></summary>
    public static string? DoNotDisturb => Glyph("\uf08d");
    /// <summary>Official icon: <c>do_not_disturb_alt</c></summary>
    public static string? DoNotDisturbAlt => Glyph("\uf08c");
    /// <summary>Official icon: <c>do_not_disturb_off</c></summary>
    public static string? DoNotDisturbOff => Glyph("\uf08e");
    /// <summary>Official icon: <c>do_not_disturb_on</c></summary>
    public static string? DoNotDisturbOn => Glyph("\uf08f");
    /// <summary>Official icon: <c>do_not_disturb_on_total_silence</c></summary>
    public static string? DoNotDisturbOnTotalSilence => Glyph("\ueffb");
    /// <summary>Official icon: <c>do_not_step</c></summary>
    public static string? DoNotStep => Glyph("\uf19f");
    /// <summary>Official icon: <c>do_not_touch</c></summary>
    public static string? DoNotTouch => Glyph("\uf1b0");
    /// <summary>Official icon: <c>dock</c></summary>
    public static string? Dock => Glyph("\uf2e0");
    /// <summary>Official icon: <c>dock_to_bottom</c></summary>
    public static string? DockToBottom => Glyph("\uf7e6");
    /// <summary>Official icon: <c>dock_to_left</c></summary>
    public static string? DockToLeft => Glyph("\uf7e5");
    /// <summary>Official icon: <c>dock_to_right</c></summary>
    public static string? DockToRight => Glyph("\uf7e4");
    /// <summary>Official icon: <c>docs</c></summary>
    public static string? Docs => Glyph("\uea7d");
    /// <summary>Official icon: <c>docs_add_on</c></summary>
    public static string? DocsAddOn => Glyph("\uf0c2");
    /// <summary>Official icon: <c>docs_apps_script</c></summary>
    public static string? DocsAppsScript => Glyph("\uf0c3");
    /// <summary>Official icon: <c>document_scanner</c></summary>
    public static string? DocumentScanner => Glyph("\ue5fa");
    /// <summary>Official icon: <c>document_search</c></summary>
    public static string? DocumentSearch => Glyph("\uf385");
    /// <summary>Official icon: <c>domain</c></summary>
    public static string? Domain => Glyph("\ue7ee");
    /// <summary>Official icon: <c>domain_add</c></summary>
    public static string? DomainAdd => Glyph("\ueb62");
    /// <summary>Official icon: <c>domain_disabled</c></summary>
    public static string? DomainDisabled => Glyph("\ue0ef");
    /// <summary>Official icon: <c>domain_disabled_check</c></summary>
    public static string? DomainDisabledCheck => Glyph("\U000FFEC6");
    /// <summary>Official icon: <c>domain_verification</c></summary>
    public static string? DomainVerification => Glyph("\uef4c");
    /// <summary>Official icon: <c>domain_verification_off</c></summary>
    public static string? DomainVerificationOff => Glyph("\uf7b0");
    /// <summary>Official icon: <c>domino_mask</c></summary>
    public static string? DominoMask => Glyph("\uf5e4");
    /// <summary>Official icon: <c>done</c></summary>
    public static string? Done => Glyph("\ue876");
    /// <summary>Official icon: <c>done_all</c></summary>
    public static string? DoneAll => Glyph("\ue877");
    /// <summary>Official icon: <c>done_outline</c></summary>
    public static string? DoneOutline => Glyph("\ue92f");
    /// <summary>Official icon: <c>donut_large</c></summary>
    public static string? DonutLarge => Glyph("\ue917");
    /// <summary>Official icon: <c>donut_small</c></summary>
    public static string? DonutSmall => Glyph("\ue918");
    /// <summary>Official icon: <c>door_back</c></summary>
    public static string? DoorBack => Glyph("\ueffc");
    /// <summary>Official icon: <c>door_front</c></summary>
    public static string? DoorFront => Glyph("\ueffd");
    /// <summary>Official icon: <c>door_open</c></summary>
    public static string? DoorOpen => Glyph("\ue77c");
    /// <summary>Official icon: <c>door_sensor</c></summary>
    public static string? DoorSensor => Glyph("\ue28a");
    /// <summary>Official icon: <c>door_sliding</c></summary>
    public static string? DoorSliding => Glyph("\ueffe");
    /// <summary>Official icon: <c>doorbell</c></summary>
    public static string? Doorbell => Glyph("\uefff");
    /// <summary>Official icon: <c>doorbell_3p</c></summary>
    public static string? Doorbell3p => Glyph("\ue1e7");
    /// <summary>Official icon: <c>doorbell_chime</c></summary>
    public static string? DoorbellChime => Glyph("\ue1f3");
    /// <summary>Official icon: <c>double_arrow</c></summary>
    public static string? DoubleArrow => Glyph("\uea50");
    /// <summary>Official icon: <c>downhill_skiing</c></summary>
    public static string? DownhillSkiing => Glyph("\ue509");
    /// <summary>Official icon: <c>download</c></summary>
    public static string? Download => Glyph("\uf090");
    /// <summary>Official icon: <c>download_2</c></summary>
    public static string? Download2 => Glyph("\uf523");
    /// <summary>Official icon: <c>download_done</c></summary>
    public static string? DownloadDone => Glyph("\uf091");
    /// <summary>Official icon: <c>download_for_offline</c></summary>
    public static string? DownloadForOffline => Glyph("\uf000");
    /// <summary>Official icon: <c>downloading</c></summary>
    public static string? Downloading => Glyph("\uf001");
    /// <summary>Official icon: <c>draft</c></summary>
    public static string? Draft => Glyph("\ue66d");
    /// <summary>Official icon: <c>draft_orders</c></summary>
    public static string? DraftOrders => Glyph("\ue7b3");
    /// <summary>Official icon: <c>drafts</c></summary>
    public static string? Drafts => Glyph("\ue151");
    /// <summary>Official icon: <c>drag_click</c></summary>
    public static string? DragClick => Glyph("\uf71f");
    /// <summary>Official icon: <c>drag_handle</c></summary>
    public static string? DragHandle => Glyph("\ue25d");
    /// <summary>Official icon: <c>drag_indicator</c></summary>
    public static string? DragIndicator => Glyph("\ue945");
    /// <summary>Official icon: <c>drag_pan</c></summary>
    public static string? DragPan => Glyph("\uf71e");
    /// <summary>Official icon: <c>draw</c></summary>
    public static string? Draw => Glyph("\ue746");
    /// <summary>Official icon: <c>draw_abstract</c></summary>
    public static string? DrawAbstract => Glyph("\uf7f8");
    /// <summary>Official icon: <c>draw_collage</c></summary>
    public static string? DrawCollage => Glyph("\uf7f7");
    /// <summary>Official icon: <c>drawing_recognition</c></summary>
    public static string? DrawingRecognition => Glyph("\ueb00");
    /// <summary>Official icon: <c>dresser</c></summary>
    public static string? Dresser => Glyph("\ue210");
    /// <summary>Official icon: <c>drive_eta</c></summary>
    public static string? DriveEta => Glyph("\ueff7");
    /// <summary>Official icon: <c>drive_export</c></summary>
    public static string? DriveExport => Glyph("\uf41d");
    /// <summary>Official icon: <c>drive_file_move</c></summary>
    public static string? DriveFileMove => Glyph("\ue9a1");
    /// <summary>Official icon: <c>drive_file_move_outline</c></summary>
    public static string? DriveFileMoveOutline => Glyph("\ue9a1");
    /// <summary>Official icon: <c>drive_file_move_rtl</c></summary>
    public static string? DriveFileMoveRtl => Glyph("\ue9a1");
    /// <summary>Official icon: <c>drive_file_rename</c></summary>
    public static string? DriveFileRename => Glyph("\ue676");
    /// <summary>Official icon: <c>drive_file_rename_outline</c></summary>
    public static string? DriveFileRenameOutline => Glyph("\ue9a2");
    /// <summary>Official icon: <c>drive_folder_upload</c></summary>
    public static string? DriveFolderUpload => Glyph("\ue9a3");
    /// <summary>Official icon: <c>drone</c></summary>
    public static string? Drone => Glyph("\uf25a");
    /// <summary>Official icon: <c>drone_2</c></summary>
    public static string? Drone2 => Glyph("\uf259");
    /// <summary>Official icon: <c>dropdown</c></summary>
    public static string? Dropdown => Glyph("\ue9a4");
    /// <summary>Official icon: <c>dropdown_menu</c></summary>
    public static string? DropdownMenu => Glyph("\U000FFEF0");
    /// <summary>Official icon: <c>dropper_eye</c></summary>
    public static string? DropperEye => Glyph("\uf351");
    /// <summary>Official icon: <c>dry</c></summary>
    public static string? Dry => Glyph("\uf1b3");
    /// <summary>Official icon: <c>dry_cleaning</c></summary>
    public static string? DryCleaning => Glyph("\uea58");
    /// <summary>Official icon: <c>dual_screen</c></summary>
    public static string? DualScreen => Glyph("\uf6cf");
    /// <summary>Official icon: <c>duo</c></summary>
    public static string? Duo => Glyph("\ue9a5");
    /// <summary>Official icon: <c>dvr</c></summary>
    public static string? Dvr => Glyph("\ue1b2");
    /// <summary>Official icon: <c>dynamic_feed</c></summary>
    public static string? DynamicFeed => Glyph("\uea14");
    /// <summary>Official icon: <c>dynamic_form</c></summary>
    public static string? DynamicForm => Glyph("\uf1bf");
    /// <summary>Official icon: <c>e911_avatar</c></summary>
    public static string? E911Avatar => Glyph("\uf11a");
    /// <summary>Official icon: <c>e911_emergency</c></summary>
    public static string? E911Emergency => Glyph("\uf119");
    /// <summary>Official icon: <c>e_mobiledata</c></summary>
    public static string? EMobiledata => Glyph("\uf002");
    /// <summary>Official icon: <c>e_mobiledata_badge</c></summary>
    public static string? EMobiledataBadge => Glyph("\uf7e3");
    /// <summary>Official icon: <c>ear_sound</c></summary>
    public static string? EarSound => Glyph("\uf356");
    /// <summary>Official icon: <c>earbud_case</c></summary>
    public static string? EarbudCase => Glyph("\uf327");
    /// <summary>Official icon: <c>earbud_left</c></summary>
    public static string? EarbudLeft => Glyph("\uf326");
    /// <summary>Official icon: <c>earbud_right</c></summary>
    public static string? EarbudRight => Glyph("\uf325");
    /// <summary>Official icon: <c>earbuds</c></summary>
    public static string? Earbuds => Glyph("\uf003");
    /// <summary>Official icon: <c>earbuds_2</c></summary>
    public static string? Earbuds2 => Glyph("\uf324");
    /// <summary>Official icon: <c>earbuds_battery</c></summary>
    public static string? EarbudsBattery => Glyph("\uf004");
    /// <summary>Official icon: <c>early_on</c></summary>
    public static string? EarlyOn => Glyph("\ue2ba");
    /// <summary>Official icon: <c>earthquake</c></summary>
    public static string? Earthquake => Glyph("\uf64f");
    /// <summary>Official icon: <c>east</c></summary>
    public static string? East => Glyph("\uf1df");
    /// <summary>Official icon: <c>ecg</c></summary>
    public static string? Ecg => Glyph("\uf80f");
    /// <summary>Official icon: <c>ecg_heart</c></summary>
    public static string? EcgHeart => Glyph("\uf6e9");
    /// <summary>Official icon: <c>eco</c></summary>
    public static string? Eco => Glyph("\uea35");
    /// <summary>Official icon: <c>eda</c></summary>
    public static string? Eda => Glyph("\uf6e8");
    /// <summary>Official icon: <c>edgesensor_high</c></summary>
    public static string? EdgesensorHigh => Glyph("\uf2ef");
    /// <summary>Official icon: <c>edgesensor_low</c></summary>
    public static string? EdgesensorLow => Glyph("\uf2ee");
    /// <summary>Official icon: <c>edit</c></summary>
    public static string? Edit => Glyph("\uf097");
    /// <summary>Official icon: <c>edit_arrow_down</c></summary>
    public static string? EditArrowDown => Glyph("\uf380");
    /// <summary>Official icon: <c>edit_arrow_up</c></summary>
    public static string? EditArrowUp => Glyph("\uf37f");
    /// <summary>Official icon: <c>edit_attributes</c></summary>
    public static string? EditAttributes => Glyph("\ue578");
    /// <summary>Official icon: <c>edit_audio</c></summary>
    public static string? EditAudio => Glyph("\uf42d");
    /// <summary>Official icon: <c>edit_calendar</c></summary>
    public static string? EditCalendar => Glyph("\ue742");
    /// <summary>Official icon: <c>edit_document</c></summary>
    public static string? EditDocument => Glyph("\uf88c");
    /// <summary>Official icon: <c>edit_location</c></summary>
    public static string? EditLocation => Glyph("\ue568");
    /// <summary>Official icon: <c>edit_location_alt</c></summary>
    public static string? EditLocationAlt => Glyph("\ue1c5");
    /// <summary>Official icon: <c>edit_note</c></summary>
    public static string? EditNote => Glyph("\ue745");
    /// <summary>Official icon: <c>edit_notifications</c></summary>
    public static string? EditNotifications => Glyph("\ue525");
    /// <summary>Official icon: <c>edit_off</c></summary>
    public static string? EditOff => Glyph("\ue950");
    /// <summary>Official icon: <c>edit_road</c></summary>
    public static string? EditRoad => Glyph("\uef4d");
    /// <summary>Official icon: <c>edit_square</c></summary>
    public static string? EditSquare => Glyph("\uf88d");
    /// <summary>Official icon: <c>editor_choice</c></summary>
    public static string? EditorChoice => Glyph("\uf528");
    /// <summary>Official icon: <c>egg</c></summary>
    public static string? Egg => Glyph("\ueacc");
    /// <summary>Official icon: <c>egg_alt</c></summary>
    public static string? EggAlt => Glyph("\ueac8");
    /// <summary>Official icon: <c>eject</c></summary>
    public static string? Eject => Glyph("\ue8fb");
    /// <summary>Official icon: <c>elderly</c></summary>
    public static string? Elderly => Glyph("\uf21a");
    /// <summary>Official icon: <c>elderly_woman</c></summary>
    public static string? ElderlyWoman => Glyph("\ueb69");
    /// <summary>Official icon: <c>electric_bike</c></summary>
    public static string? ElectricBike => Glyph("\ueb1b");
    /// <summary>Official icon: <c>electric_bolt</c></summary>
    public static string? ElectricBolt => Glyph("\uec1c");
    /// <summary>Official icon: <c>electric_car</c></summary>
    public static string? ElectricCar => Glyph("\ueb1c");
    /// <summary>Official icon: <c>electric_meter</c></summary>
    public static string? ElectricMeter => Glyph("\uec1b");
    /// <summary>Official icon: <c>electric_moped</c></summary>
    public static string? ElectricMoped => Glyph("\ueb1d");
    /// <summary>Official icon: <c>electric_rickshaw</c></summary>
    public static string? ElectricRickshaw => Glyph("\ueb1e");
    /// <summary>Official icon: <c>electric_scooter</c></summary>
    public static string? ElectricScooter => Glyph("\ueb1f");
    /// <summary>Official icon: <c>electrical_services</c></summary>
    public static string? ElectricalServices => Glyph("\uf102");
    /// <summary>Official icon: <c>elevation</c></summary>
    public static string? Elevation => Glyph("\uf6e7");
    /// <summary>Official icon: <c>elevator</c></summary>
    public static string? Elevator => Glyph("\uf1a0");
    /// <summary>Official icon: <c>email</c></summary>
    public static string? Email => Glyph("\ue159");
    /// <summary>Official icon: <c>emergency</c></summary>
    public static string? Emergency => Glyph("\ue1eb");
    /// <summary>Official icon: <c>emergency_heat</c></summary>
    public static string? EmergencyHeat => Glyph("\uf15d");
    /// <summary>Official icon: <c>emergency_heat_2</c></summary>
    public static string? EmergencyHeat2 => Glyph("\uf4e5");
    /// <summary>Official icon: <c>emergency_home</c></summary>
    public static string? EmergencyHome => Glyph("\ue82a");
    /// <summary>Official icon: <c>emergency_recording</c></summary>
    public static string? EmergencyRecording => Glyph("\uebf4");
    /// <summary>Official icon: <c>emergency_share</c></summary>
    public static string? EmergencyShare => Glyph("\uebf6");
    /// <summary>Official icon: <c>emergency_share_off</c></summary>
    public static string? EmergencyShareOff => Glyph("\uf59e");
    /// <summary>Official icon: <c>emoji_emotions</c></summary>
    public static string? EmojiEmotions => Glyph("\uea22");
    /// <summary>Official icon: <c>emoji_events</c></summary>
    public static string? EmojiEvents => Glyph("\uea23");
    /// <summary>Official icon: <c>emoji_flags</c></summary>
    public static string? EmojiFlags => Glyph("\uf0c6");
    /// <summary>Official icon: <c>emoji_food_beverage</c></summary>
    public static string? EmojiFoodBeverage => Glyph("\uea1b");
    /// <summary>Official icon: <c>emoji_language</c></summary>
    public static string? EmojiLanguage => Glyph("\uf4cd");
    /// <summary>Official icon: <c>emoji_nature</c></summary>
    public static string? EmojiNature => Glyph("\uea1c");
    /// <summary>Official icon: <c>emoji_objects</c></summary>
    public static string? EmojiObjects => Glyph("\uea24");
    /// <summary>Official icon: <c>emoji_people</c></summary>
    public static string? EmojiPeople => Glyph("\uea1d");
    /// <summary>Official icon: <c>emoji_symbols</c></summary>
    public static string? EmojiSymbols => Glyph("\uea1e");
    /// <summary>Official icon: <c>emoji_transportation</c></summary>
    public static string? EmojiTransportation => Glyph("\uea1f");
    /// <summary>Official icon: <c>emoticon</c></summary>
    public static string? Emoticon => Glyph("\ue5f3");
    /// <summary>Official icon: <c>empty_dashboard</c></summary>
    public static string? EmptyDashboard => Glyph("\uf844");
    /// <summary>Official icon: <c>enable</c></summary>
    public static string? Enable => Glyph("\uf188");
    /// <summary>Official icon: <c>encrypted</c></summary>
    public static string? Encrypted => Glyph("\ue593");
    /// <summary>Official icon: <c>encrypted_add</c></summary>
    public static string? EncryptedAdd => Glyph("\uf429");
    /// <summary>Official icon: <c>encrypted_add_circle</c></summary>
    public static string? EncryptedAddCircle => Glyph("\uf42a");
    /// <summary>Official icon: <c>encrypted_minus_circle</c></summary>
    public static string? EncryptedMinusCircle => Glyph("\uf428");
    /// <summary>Official icon: <c>encrypted_off</c></summary>
    public static string? EncryptedOff => Glyph("\uf427");
    /// <summary>Official icon: <c>endocrinology</c></summary>
    public static string? Endocrinology => Glyph("\ue0a9");
    /// <summary>Official icon: <c>energy</c></summary>
    public static string? Energy => Glyph("\ue9a6");
    /// <summary>Official icon: <c>energy_program_saving</c></summary>
    public static string? EnergyProgramSaving => Glyph("\uf15f");
    /// <summary>Official icon: <c>energy_program_time_used</c></summary>
    public static string? EnergyProgramTimeUsed => Glyph("\uf161");
    /// <summary>Official icon: <c>energy_savings_leaf</c></summary>
    public static string? EnergySavingsLeaf => Glyph("\uec1a");
    /// <summary>Official icon: <c>engineering</c></summary>
    public static string? Engineering => Glyph("\uea3d");
    /// <summary>Official icon: <c>enhanced_encryption</c></summary>
    public static string? EnhancedEncryption => Glyph("\ue63f");
    /// <summary>Official icon: <c>ent</c></summary>
    public static string? Ent => Glyph("\ue0aa");
    /// <summary>Official icon: <c>enterprise</c></summary>
    public static string? Enterprise => Glyph("\ue70e");
    /// <summary>Official icon: <c>enterprise_off</c></summary>
    public static string? EnterpriseOff => Glyph("\ueb4d");
    /// <summary>Official icon: <c>equal</c></summary>
    public static string? Equal => Glyph("\uf77b");
    /// <summary>Official icon: <c>equalizer</c></summary>
    public static string? Equalizer => Glyph("\ue01d");
    /// <summary>Official icon: <c>eraser_size_1</c></summary>
    public static string? EraserSize1 => Glyph("\uf3fc");
    /// <summary>Official icon: <c>eraser_size_2</c></summary>
    public static string? EraserSize2 => Glyph("\uf3fb");
    /// <summary>Official icon: <c>eraser_size_3</c></summary>
    public static string? EraserSize3 => Glyph("\uf3fa");
    /// <summary>Official icon: <c>eraser_size_4</c></summary>
    public static string? EraserSize4 => Glyph("\uf3f9");
    /// <summary>Official icon: <c>eraser_size_5</c></summary>
    public static string? EraserSize5 => Glyph("\uf3f8");
    /// <summary>Official icon: <c>error</c></summary>
    public static string? Error => Glyph("\uf8b6");
    /// <summary>Official icon: <c>error_circle_rounded</c></summary>
    public static string? ErrorCircleRounded => Glyph("\uf8b6");
    /// <summary>Official icon: <c>error_med</c></summary>
    public static string? ErrorMed => Glyph("\ue49b");
    /// <summary>Official icon: <c>error_outline</c></summary>
    public static string? ErrorOutline => Glyph("\uf8b6");
    /// <summary>Official icon: <c>escalator</c></summary>
    public static string? Escalator => Glyph("\uf1a1");
    /// <summary>Official icon: <c>escalator_warning</c></summary>
    public static string? EscalatorWarning => Glyph("\uf1ac");
    /// <summary>Official icon: <c>euro</c></summary>
    public static string? Euro => Glyph("\uea15");
    /// <summary>Official icon: <c>euro_symbol</c></summary>
    public static string? EuroSymbol => Glyph("\ue926");
    /// <summary>Official icon: <c>ev_charger</c></summary>
    public static string? EvCharger => Glyph("\ue56d");
    /// <summary>Official icon: <c>ev_mobiledata_badge</c></summary>
    public static string? EvMobiledataBadge => Glyph("\uf7e2");
    /// <summary>Official icon: <c>ev_shadow</c></summary>
    public static string? EvShadow => Glyph("\uef8f");
    /// <summary>Official icon: <c>ev_shadow_add</c></summary>
    public static string? EvShadowAdd => Glyph("\uf580");
    /// <summary>Official icon: <c>ev_shadow_minus</c></summary>
    public static string? EvShadowMinus => Glyph("\uf57f");
    /// <summary>Official icon: <c>ev_station</c></summary>
    public static string? EvStation => Glyph("\ue56d");
    /// <summary>Official icon: <c>event</c></summary>
    public static string? Event => Glyph("\ue878");
    /// <summary>Official icon: <c>event_available</c></summary>
    public static string? EventAvailable => Glyph("\ue614");
    /// <summary>Official icon: <c>event_busy</c></summary>
    public static string? EventBusy => Glyph("\ue615");
    /// <summary>Official icon: <c>event_list</c></summary>
    public static string? EventList => Glyph("\uf683");
    /// <summary>Official icon: <c>event_note</c></summary>
    public static string? EventNote => Glyph("\ue616");
    /// <summary>Official icon: <c>event_repeat</c></summary>
    public static string? EventRepeat => Glyph("\ueb7b");
    /// <summary>Official icon: <c>event_seat</c></summary>
    public static string? EventSeat => Glyph("\ue903");
    /// <summary>Official icon: <c>event_upcoming</c></summary>
    public static string? EventUpcoming => Glyph("\uf238");
    /// <summary>Official icon: <c>exclamation</c></summary>
    public static string? Exclamation => Glyph("\uf22f");
    /// <summary>Official icon: <c>exercise</c></summary>
    public static string? Exercise => Glyph("\uf6e6");
    /// <summary>Official icon: <c>exit_to_app</c></summary>
    public static string? ExitToApp => Glyph("\ue879");
    /// <summary>Official icon: <c>expand</c></summary>
    public static string? Expand => Glyph("\ue94f");
    /// <summary>Official icon: <c>expand_all</c></summary>
    public static string? ExpandAll => Glyph("\ue946");
    /// <summary>Official icon: <c>expand_circle_down</c></summary>
    public static string? ExpandCircleDown => Glyph("\ue7cd");
    /// <summary>Official icon: <c>expand_circle_right</c></summary>
    public static string? ExpandCircleRight => Glyph("\uf591");
    /// <summary>Official icon: <c>expand_circle_up</c></summary>
    public static string? ExpandCircleUp => Glyph("\uf5d2");
    /// <summary>Official icon: <c>expand_content</c></summary>
    public static string? ExpandContent => Glyph("\uf830");
    /// <summary>Official icon: <c>expand_less</c></summary>
    public static string? ExpandLess => Glyph("\ue5ce");
    /// <summary>Official icon: <c>expand_more</c></summary>
    public static string? ExpandMore => Glyph("\ue5cf");
    /// <summary>Official icon: <c>expansion_panels</c></summary>
    public static string? ExpansionPanels => Glyph("\uef90");
    /// <summary>Official icon: <c>expension_panels</c></summary>
    public static string? ExpensionPanels => Glyph("\uef90");
    /// <summary>Official icon: <c>experiment</c></summary>
    public static string? Experiment => Glyph("\ue686");
    /// <summary>Official icon: <c>explicit</c></summary>
    public static string? Explicit => Glyph("\ue01e");
    /// <summary>Official icon: <c>explore</c></summary>
    public static string? Explore => Glyph("\ue87a");
    /// <summary>Official icon: <c>explore_nearby</c></summary>
    public static string? ExploreNearby => Glyph("\ue538");
    /// <summary>Official icon: <c>explore_off</c></summary>
    public static string? ExploreOff => Glyph("\ue9a8");
    /// <summary>Official icon: <c>explosion</c></summary>
    public static string? Explosion => Glyph("\uf685");
    /// <summary>Official icon: <c>export_notes</c></summary>
    public static string? ExportNotes => Glyph("\ue0ac");
    /// <summary>Official icon: <c>exposure</c></summary>
    public static string? Exposure => Glyph("\ue3f6");
    /// <summary>Official icon: <c>exposure_neg_1</c></summary>
    public static string? ExposureNeg1 => Glyph("\ue3cb");
    /// <summary>Official icon: <c>exposure_neg_2</c></summary>
    public static string? ExposureNeg2 => Glyph("\ue3cc");
    /// <summary>Official icon: <c>exposure_plus_1</c></summary>
    public static string? ExposurePlus1 => Glyph("\ue800");
    /// <summary>Official icon: <c>exposure_plus_2</c></summary>
    public static string? ExposurePlus2 => Glyph("\ue3ce");
    /// <summary>Official icon: <c>exposure_zero</c></summary>
    public static string? ExposureZero => Glyph("\ue3cf");
    /// <summary>Official icon: <c>extension</c></summary>
    public static string? Extension => Glyph("\ue87b");
    /// <summary>Official icon: <c>extension_off</c></summary>
    public static string? ExtensionOff => Glyph("\ue4f5");
    /// <summary>Official icon: <c>eye_tracking</c></summary>
    public static string? EyeTracking => Glyph("\uf4c9");
    /// <summary>Official icon: <c>eyebrow</c></summary>
    public static string? Eyebrow => Glyph("\ueeb3");
    /// <summary>Official icon: <c>eyeglasses</c></summary>
    public static string? Eyeglasses => Glyph("\uf6ee");
    /// <summary>Official icon: <c>eyeglasses_2</c></summary>
    public static string? Eyeglasses2 => Glyph("\uf2c7");
    /// <summary>Official icon: <c>eyeglasses_2_sound</c></summary>
    public static string? Eyeglasses2Sound => Glyph("\uf265");
    /// <summary>Official icon: <c>eyeglasses_3</c></summary>
    public static string? Eyeglasses3 => Glyph("\U000FFEF1");
    /// <summary>Official icon: <c>face</c></summary>
    public static string? Face => Glyph("\uf008");
    /// <summary>Official icon: <c>face_2</c></summary>
    public static string? Face2 => Glyph("\uf8da");
    /// <summary>Official icon: <c>face_3</c></summary>
    public static string? Face3 => Glyph("\uf8db");
    /// <summary>Official icon: <c>face_4</c></summary>
    public static string? Face4 => Glyph("\uf8dc");
    /// <summary>Official icon: <c>face_5</c></summary>
    public static string? Face5 => Glyph("\uf8dd");
    /// <summary>Official icon: <c>face_6</c></summary>
    public static string? Face6 => Glyph("\uf8de");
    /// <summary>Official icon: <c>face_down</c></summary>
    public static string? FaceDown => Glyph("\uf402");
    /// <summary>Official icon: <c>face_left</c></summary>
    public static string? FaceLeft => Glyph("\uf401");
    /// <summary>Official icon: <c>face_nod</c></summary>
    public static string? FaceNod => Glyph("\uf400");
    /// <summary>Official icon: <c>face_retouching_natural</c></summary>
    public static string? FaceRetouchingNatural => Glyph("\uef4e");
    /// <summary>Official icon: <c>face_retouching_off</c></summary>
    public static string? FaceRetouchingOff => Glyph("\uf007");
    /// <summary>Official icon: <c>face_right</c></summary>
    public static string? FaceRight => Glyph("\uf3ff");
    /// <summary>Official icon: <c>face_shake</c></summary>
    public static string? FaceShake => Glyph("\uf3fe");
    /// <summary>Official icon: <c>face_unlock</c></summary>
    public static string? FaceUnlock => Glyph("\uf008");
    /// <summary>Official icon: <c>face_up</c></summary>
    public static string? FaceUp => Glyph("\uf3fd");
    /// <summary>Official icon: <c>fact_check</c></summary>
    public static string? FactCheck => Glyph("\uf0c5");
    /// <summary>Official icon: <c>factory</c></summary>
    public static string? Factory => Glyph("\uebbc");
    /// <summary>Official icon: <c>falling</c></summary>
    public static string? Falling => Glyph("\uf60d");
    /// <summary>Official icon: <c>familiar_face_and_zone</c></summary>
    public static string? FamiliarFaceAndZone => Glyph("\ue21c");
    /// <summary>Official icon: <c>family_group</c></summary>
    public static string? FamilyGroup => Glyph("\ueef2");
    /// <summary>Official icon: <c>family_history</c></summary>
    public static string? FamilyHistory => Glyph("\ue0ad");
    /// <summary>Official icon: <c>family_home</c></summary>
    public static string? FamilyHome => Glyph("\ueb26");
    /// <summary>Official icon: <c>family_link</c></summary>
    public static string? FamilyLink => Glyph("\ueb19");
    /// <summary>Official icon: <c>family_restroom</c></summary>
    public static string? FamilyRestroom => Glyph("\uf1a2");
    /// <summary>Official icon: <c>family_star</c></summary>
    public static string? FamilyStar => Glyph("\uf527");
    /// <summary>Official icon: <c>fan_focus</c></summary>
    public static string? FanFocus => Glyph("\uf334");
    /// <summary>Official icon: <c>fan_indirect</c></summary>
    public static string? FanIndirect => Glyph("\uf333");
    /// <summary>Official icon: <c>farsight_digital</c></summary>
    public static string? FarsightDigital => Glyph("\uf559");
    /// <summary>Official icon: <c>fast_forward</c></summary>
    public static string? FastForward => Glyph("\ue01f");
    /// <summary>Official icon: <c>fast_rewind</c></summary>
    public static string? FastRewind => Glyph("\ue020");
    /// <summary>Official icon: <c>fastfood</c></summary>
    public static string? Fastfood => Glyph("\ue57a");
    /// <summary>Official icon: <c>faucet</c></summary>
    public static string? Faucet => Glyph("\ue278");
    /// <summary>Official icon: <c>favorite</c></summary>
    public static string? Favorite => Glyph("\ue87e");
    /// <summary>Official icon: <c>favorite_border</c></summary>
    public static string? FavoriteBorder => Glyph("\ue87e");
    /// <summary>Official icon: <c>fax</c></summary>
    public static string? Fax => Glyph("\uead8");
    /// <summary>Official icon: <c>feature_search</c></summary>
    public static string? FeatureSearch => Glyph("\ue9a9");
    /// <summary>Official icon: <c>featured_play_list</c></summary>
    public static string? FeaturedPlayList => Glyph("\ue06d");
    /// <summary>Official icon: <c>featured_seasonal_and_gifts</c></summary>
    public static string? FeaturedSeasonalAndGifts => Glyph("\uef91");
    /// <summary>Official icon: <c>featured_video</c></summary>
    public static string? FeaturedVideo => Glyph("\ue06e");
    /// <summary>Official icon: <c>feed</c></summary>
    public static string? Feed => Glyph("\uf009");
    /// <summary>Official icon: <c>feedback</c></summary>
    public static string? Feedback => Glyph("\ue87f");
    /// <summary>Official icon: <c>female</c></summary>
    public static string? Female => Glyph("\ue590");
    /// <summary>Official icon: <c>femur</c></summary>
    public static string? Femur => Glyph("\uf891");
    /// <summary>Official icon: <c>femur_alt</c></summary>
    public static string? FemurAlt => Glyph("\uf892");
    /// <summary>Official icon: <c>fence</c></summary>
    public static string? Fence => Glyph("\uf1f6");
    /// <summary>Official icon: <c>fertile</c></summary>
    public static string? Fertile => Glyph("\uf6e5");
    /// <summary>Official icon: <c>festival</c></summary>
    public static string? Festival => Glyph("\uea68");
    /// <summary>Official icon: <c>fiber_dvr</c></summary>
    public static string? FiberDvr => Glyph("\ue05d");
    /// <summary>Official icon: <c>fiber_manual_record</c></summary>
    public static string? FiberManualRecord => Glyph("\ue061");
    /// <summary>Official icon: <c>fiber_new</c></summary>
    public static string? FiberNew => Glyph("\ue05e");
    /// <summary>Official icon: <c>fiber_pin</c></summary>
    public static string? FiberPin => Glyph("\ue06a");
    /// <summary>Official icon: <c>fiber_smart_record</c></summary>
    public static string? FiberSmartRecord => Glyph("\ue062");
    /// <summary>Official icon: <c>file_copy</c></summary>
    public static string? FileCopy => Glyph("\ue173");
    /// <summary>Official icon: <c>file_copy_off</c></summary>
    public static string? FileCopyOff => Glyph("\uf4d8");
    /// <summary>Official icon: <c>file_download</c></summary>
    public static string? FileDownload => Glyph("\uf090");
    /// <summary>Official icon: <c>file_download_done</c></summary>
    public static string? FileDownloadDone => Glyph("\uf091");
    /// <summary>Official icon: <c>file_download_off</c></summary>
    public static string? FileDownloadOff => Glyph("\ue4fe");
    /// <summary>Official icon: <c>file_export</c></summary>
    public static string? FileExport => Glyph("\uf3b2");
    /// <summary>Official icon: <c>file_json</c></summary>
    public static string? FileJson => Glyph("\uf3bb");
    /// <summary>Official icon: <c>file_map</c></summary>
    public static string? FileMap => Glyph("\ue2c5");
    /// <summary>Official icon: <c>file_map_stack</c></summary>
    public static string? FileMapStack => Glyph("\uf3e2");
    /// <summary>Official icon: <c>file_open</c></summary>
    public static string? FileOpen => Glyph("\ueaf3");
    /// <summary>Official icon: <c>file_png</c></summary>
    public static string? FilePng => Glyph("\uf3bc");
    /// <summary>Official icon: <c>file_present</c></summary>
    public static string? FilePresent => Glyph("\uea0e");
    /// <summary>Official icon: <c>file_save</c></summary>
    public static string? FileSave => Glyph("\uf17f");
    /// <summary>Official icon: <c>file_save_off</c></summary>
    public static string? FileSaveOff => Glyph("\ue505");
    /// <summary>Official icon: <c>file_upload</c></summary>
    public static string? FileUpload => Glyph("\uf09b");
    /// <summary>Official icon: <c>file_upload_off</c></summary>
    public static string? FileUploadOff => Glyph("\uf886");
    /// <summary>Official icon: <c>files</c></summary>
    public static string? Files => Glyph("\uea85");
    /// <summary>Official icon: <c>filter</c></summary>
    public static string? Filter => Glyph("\ue3d3");
    /// <summary>Official icon: <c>filter_1</c></summary>
    public static string? Filter1 => Glyph("\ue3d0");
    /// <summary>Official icon: <c>filter_2</c></summary>
    public static string? Filter2 => Glyph("\ue3d1");
    /// <summary>Official icon: <c>filter_3</c></summary>
    public static string? Filter3 => Glyph("\ue3d2");
    /// <summary>Official icon: <c>filter_4</c></summary>
    public static string? Filter4 => Glyph("\ue3d4");
    /// <summary>Official icon: <c>filter_5</c></summary>
    public static string? Filter5 => Glyph("\ue3d5");
    /// <summary>Official icon: <c>filter_6</c></summary>
    public static string? Filter6 => Glyph("\ue3d6");
    /// <summary>Official icon: <c>filter_7</c></summary>
    public static string? Filter7 => Glyph("\ue3d7");
    /// <summary>Official icon: <c>filter_8</c></summary>
    public static string? Filter8 => Glyph("\ue3d8");
    /// <summary>Official icon: <c>filter_9</c></summary>
    public static string? Filter9 => Glyph("\ue3d9");
    /// <summary>Official icon: <c>filter_9_plus</c></summary>
    public static string? Filter9Plus => Glyph("\ue3da");
    /// <summary>Official icon: <c>filter_alt</c></summary>
    public static string? FilterAlt => Glyph("\uef4f");
    /// <summary>Official icon: <c>filter_alt_off</c></summary>
    public static string? FilterAltOff => Glyph("\ueb32");
    /// <summary>Official icon: <c>filter_arrow_right</c></summary>
    public static string? FilterArrowRight => Glyph("\uf3d1");
    /// <summary>Official icon: <c>filter_b_and_w</c></summary>
    public static string? FilterBAndW => Glyph("\ue3db");
    /// <summary>Official icon: <c>filter_center_focus</c></summary>
    public static string? FilterCenterFocus => Glyph("\ue3dc");
    /// <summary>Official icon: <c>filter_drama</c></summary>
    public static string? FilterDrama => Glyph("\ue3dd");
    /// <summary>Official icon: <c>filter_frames</c></summary>
    public static string? FilterFrames => Glyph("\ue3de");
    /// <summary>Official icon: <c>filter_hdr</c></summary>
    public static string? FilterHdr => Glyph("\ue3df");
    /// <summary>Official icon: <c>filter_list</c></summary>
    public static string? FilterList => Glyph("\ue152");
    /// <summary>Official icon: <c>filter_list_alt</c></summary>
    public static string? FilterListAlt => Glyph("\ue94e");
    /// <summary>Official icon: <c>filter_list_off</c></summary>
    public static string? FilterListOff => Glyph("\ueb57");
    /// <summary>Official icon: <c>filter_none</c></summary>
    public static string? FilterNone => Glyph("\ue3e0");
    /// <summary>Official icon: <c>filter_retrolux</c></summary>
    public static string? FilterRetrolux => Glyph("\ue3e1");
    /// <summary>Official icon: <c>filter_tilt_shift</c></summary>
    public static string? FilterTiltShift => Glyph("\ue3e2");
    /// <summary>Official icon: <c>filter_vintage</c></summary>
    public static string? FilterVintage => Glyph("\ue3e3");
    /// <summary>Official icon: <c>finance</c></summary>
    public static string? Finance => Glyph("\ue6bf");
    /// <summary>Official icon: <c>finance_chip</c></summary>
    public static string? FinanceChip => Glyph("\uf84e");
    /// <summary>Official icon: <c>finance_mode</c></summary>
    public static string? FinanceMode => Glyph("\uef92");
    /// <summary>Official icon: <c>find_in_page</c></summary>
    public static string? FindInPage => Glyph("\ue880");
    /// <summary>Official icon: <c>find_replace</c></summary>
    public static string? FindReplace => Glyph("\ue881");
    /// <summary>Official icon: <c>fingerprint</c></summary>
    public static string? Fingerprint => Glyph("\ue90d");
    /// <summary>Official icon: <c>fingerprint_off</c></summary>
    public static string? FingerprintOff => Glyph("\uf49d");
    /// <summary>Official icon: <c>fire_check</c></summary>
    public static string? FireCheck => Glyph("\U000FFFA8");
    /// <summary>Official icon: <c>fire_extinguisher</c></summary>
    public static string? FireExtinguisher => Glyph("\uf1d8");
    /// <summary>Official icon: <c>fire_hydrant</c></summary>
    public static string? FireHydrant => Glyph("\uf1a3");
    /// <summary>Official icon: <c>fire_truck</c></summary>
    public static string? FireTruck => Glyph("\uf8f2");
    /// <summary>Official icon: <c>fireplace</c></summary>
    public static string? Fireplace => Glyph("\uea43");
    /// <summary>Official icon: <c>first_page</c></summary>
    public static string? FirstPage => Glyph("\ue5dc");
    /// <summary>Official icon: <c>fit_page</c></summary>
    public static string? FitPage => Glyph("\uf77a");
    /// <summary>Official icon: <c>fit_page_height</c></summary>
    public static string? FitPageHeight => Glyph("\uf397");
    /// <summary>Official icon: <c>fit_page_width</c></summary>
    public static string? FitPageWidth => Glyph("\uf396");
    /// <summary>Official icon: <c>fit_screen</c></summary>
    public static string? FitScreen => Glyph("\uea10");
    /// <summary>Official icon: <c>fit_width</c></summary>
    public static string? FitWidth => Glyph("\uf779");
    /// <summary>Official icon: <c>fitness_center</c></summary>
    public static string? FitnessCenter => Glyph("\ueb43");
    /// <summary>Official icon: <c>fitness_tracker</c></summary>
    public static string? FitnessTracker => Glyph("\uf463");
    /// <summary>Official icon: <c>fitness_trackers</c></summary>
    public static string? FitnessTrackers => Glyph("\ueef1");
    /// <summary>Official icon: <c>flag</c></summary>
    public static string? Flag => Glyph("\uf0c6");
    /// <summary>Official icon: <c>flag_2</c></summary>
    public static string? Flag2 => Glyph("\uf40f");
    /// <summary>Official icon: <c>flag_check</c></summary>
    public static string? FlagCheck => Glyph("\uf3d8");
    /// <summary>Official icon: <c>flag_circle</c></summary>
    public static string? FlagCircle => Glyph("\ueaf8");
    /// <summary>Official icon: <c>flag_filled</c></summary>
    public static string? FlagFilled => Glyph("\uf0c6");
    /// <summary>Official icon: <c>flaky</c></summary>
    public static string? Flaky => Glyph("\uef50");
    /// <summary>Official icon: <c>flare</c></summary>
    public static string? Flare => Glyph("\ue3e4");
    /// <summary>Official icon: <c>flash_auto</c></summary>
    public static string? FlashAuto => Glyph("\ue3e5");
    /// <summary>Official icon: <c>flash_off</c></summary>
    public static string? FlashOff => Glyph("\ue3e6");
    /// <summary>Official icon: <c>flash_on</c></summary>
    public static string? FlashOn => Glyph("\ue3e7");
    /// <summary>Official icon: <c>flashlight_off</c></summary>
    public static string? FlashlightOff => Glyph("\uf00a");
    /// <summary>Official icon: <c>flashlight_on</c></summary>
    public static string? FlashlightOn => Glyph("\uf00b");
    /// <summary>Official icon: <c>flatware</c></summary>
    public static string? Flatware => Glyph("\uf00c");
    /// <summary>Official icon: <c>flex_direction</c></summary>
    public static string? FlexDirection => Glyph("\uf778");
    /// <summary>Official icon: <c>flex_no_wrap</c></summary>
    public static string? FlexNoWrap => Glyph("\uf777");
    /// <summary>Official icon: <c>flex_wrap</c></summary>
    public static string? FlexWrap => Glyph("\uf776");
    /// <summary>Official icon: <c>flight</c></summary>
    public static string? Flight => Glyph("\ue539");
    /// <summary>Official icon: <c>flight_class</c></summary>
    public static string? FlightClass => Glyph("\ue7cb");
    /// <summary>Official icon: <c>flight_land</c></summary>
    public static string? FlightLand => Glyph("\ue904");
    /// <summary>Official icon: <c>flight_takeoff</c></summary>
    public static string? FlightTakeoff => Glyph("\ue905");
    /// <summary>Official icon: <c>flights_and_hotels</c></summary>
    public static string? FlightsAndHotels => Glyph("\ue9ab");
    /// <summary>Official icon: <c>flightsmode</c></summary>
    public static string? Flightsmode => Glyph("\uef93");
    /// <summary>Official icon: <c>flip</c></summary>
    public static string? Flip => Glyph("\ue3e8");
    /// <summary>Official icon: <c>flip_camera_android</c></summary>
    public static string? FlipCameraAndroid => Glyph("\uea37");
    /// <summary>Official icon: <c>flip_camera_ios</c></summary>
    public static string? FlipCameraIos => Glyph("\uea38");
    /// <summary>Official icon: <c>flip_to_back</c></summary>
    public static string? FlipToBack => Glyph("\ue882");
    /// <summary>Official icon: <c>flip_to_front</c></summary>
    public static string? FlipToFront => Glyph("\ue883");
    /// <summary>Official icon: <c>float_landscape_2</c></summary>
    public static string? FloatLandscape2 => Glyph("\uf45c");
    /// <summary>Official icon: <c>float_portrait_2</c></summary>
    public static string? FloatPortrait2 => Glyph("\uf45b");
    /// <summary>Official icon: <c>flood</c></summary>
    public static string? Flood => Glyph("\uebe6");
    /// <summary>Official icon: <c>floor</c></summary>
    public static string? Floor => Glyph("\uf6e4");
    /// <summary>Official icon: <c>floor_lamp</c></summary>
    public static string? FloorLamp => Glyph("\ue21e");
    /// <summary>Official icon: <c>flourescent</c></summary>
    public static string? Flourescent => Glyph("\uf07d");
    /// <summary>Official icon: <c>flowchart</c></summary>
    public static string? Flowchart => Glyph("\uf38d");
    /// <summary>Official icon: <c>flowsheet</c></summary>
    public static string? Flowsheet => Glyph("\ue0ae");
    /// <summary>Official icon: <c>fluid</c></summary>
    public static string? Fluid => Glyph("\ue483");
    /// <summary>Official icon: <c>fluid_balance</c></summary>
    public static string? FluidBalance => Glyph("\uf80d");
    /// <summary>Official icon: <c>fluid_med</c></summary>
    public static string? FluidMed => Glyph("\uf80c");
    /// <summary>Official icon: <c>fluorescent</c></summary>
    public static string? Fluorescent => Glyph("\uf07d");
    /// <summary>Official icon: <c>flutter</c></summary>
    public static string? Flutter => Glyph("\uf1dd");
    /// <summary>Official icon: <c>flutter_dash</c></summary>
    public static string? FlutterDash => Glyph("\ue00b");
    /// <summary>Official icon: <c>flyover</c></summary>
    public static string? Flyover => Glyph("\uf478");
    /// <summary>Official icon: <c>fmd_bad</c></summary>
    public static string? FmdBad => Glyph("\uf00e");
    /// <summary>Official icon: <c>fmd_good</c></summary>
    public static string? FmdGood => Glyph("\uf1db");
    /// <summary>Official icon: <c>foggy</c></summary>
    public static string? Foggy => Glyph("\ue818");
    /// <summary>Official icon: <c>folded_hands</c></summary>
    public static string? FoldedHands => Glyph("\uf5ed");
    /// <summary>Official icon: <c>folder</c></summary>
    public static string? Folder => Glyph("\ue2c7");
    /// <summary>Official icon: <c>folder_check</c></summary>
    public static string? FolderCheck => Glyph("\uf3d7");
    /// <summary>Official icon: <c>folder_check_2</c></summary>
    public static string? FolderCheck2 => Glyph("\uf3d6");
    /// <summary>Official icon: <c>folder_code</c></summary>
    public static string? FolderCode => Glyph("\uf3c8");
    /// <summary>Official icon: <c>folder_copy</c></summary>
    public static string? FolderCopy => Glyph("\uebbd");
    /// <summary>Official icon: <c>folder_data</c></summary>
    public static string? FolderData => Glyph("\uf586");
    /// <summary>Official icon: <c>folder_delete</c></summary>
    public static string? FolderDelete => Glyph("\ueb34");
    /// <summary>Official icon: <c>folder_eye</c></summary>
    public static string? FolderEye => Glyph("\uf3d5");
    /// <summary>Official icon: <c>folder_info</c></summary>
    public static string? FolderInfo => Glyph("\uf395");
    /// <summary>Official icon: <c>folder_limited</c></summary>
    public static string? FolderLimited => Glyph("\uf4e4");
    /// <summary>Official icon: <c>folder_managed</c></summary>
    public static string? FolderManaged => Glyph("\uf775");
    /// <summary>Official icon: <c>folder_match</c></summary>
    public static string? FolderMatch => Glyph("\uf3d4");
    /// <summary>Official icon: <c>folder_off</c></summary>
    public static string? FolderOff => Glyph("\ueb83");
    /// <summary>Official icon: <c>folder_open</c></summary>
    public static string? FolderOpen => Glyph("\ue2c8");
    /// <summary>Official icon: <c>folder_shared</c></summary>
    public static string? FolderShared => Glyph("\ue2c9");
    /// <summary>Official icon: <c>folder_special</c></summary>
    public static string? FolderSpecial => Glyph("\ue617");
    /// <summary>Official icon: <c>folder_supervised</c></summary>
    public static string? FolderSupervised => Glyph("\uf774");
    /// <summary>Official icon: <c>folder_zip</c></summary>
    public static string? FolderZip => Glyph("\ueb2c");
    /// <summary>Official icon: <c>follow_the_signs</c></summary>
    public static string? FollowTheSigns => Glyph("\uf222");
    /// <summary>Official icon: <c>font_download</c></summary>
    public static string? FontDownload => Glyph("\ue167");
    /// <summary>Official icon: <c>font_download_off</c></summary>
    public static string? FontDownloadOff => Glyph("\ue4f9");
    /// <summary>Official icon: <c>food_bank</c></summary>
    public static string? FoodBank => Glyph("\uf1f2");
    /// <summary>Official icon: <c>foot_bones</c></summary>
    public static string? FootBones => Glyph("\uf893");
    /// <summary>Official icon: <c>footprint</c></summary>
    public static string? Footprint => Glyph("\uf87d");
    /// <summary>Official icon: <c>for_you</c></summary>
    public static string? ForYou => Glyph("\ue9ac");
    /// <summary>Official icon: <c>forest</c></summary>
    public static string? Forest => Glyph("\uea99");
    /// <summary>Official icon: <c>fork_chart</c></summary>
    public static string? ForkChart => Glyph("\U000FFFA6");
    /// <summary>Official icon: <c>fork_left</c></summary>
    public static string? ForkLeft => Glyph("\ueba0");
    /// <summary>Official icon: <c>fork_right</c></summary>
    public static string? ForkRight => Glyph("\uebac");
    /// <summary>Official icon: <c>fork_spoon</c></summary>
    public static string? ForkSpoon => Glyph("\uf3e4");
    /// <summary>Official icon: <c>forklift</c></summary>
    public static string? Forklift => Glyph("\uf868");
    /// <summary>Official icon: <c>format_align_center</c></summary>
    public static string? FormatAlignCenter => Glyph("\ue234");
    /// <summary>Official icon: <c>format_align_justify</c></summary>
    public static string? FormatAlignJustify => Glyph("\ue235");
    /// <summary>Official icon: <c>format_align_left</c></summary>
    public static string? FormatAlignLeft => Glyph("\ue236");
    /// <summary>Official icon: <c>format_align_right</c></summary>
    public static string? FormatAlignRight => Glyph("\ue237");
    /// <summary>Official icon: <c>format_bold</c></summary>
    public static string? FormatBold => Glyph("\ue238");
    /// <summary>Official icon: <c>format_clear</c></summary>
    public static string? FormatClear => Glyph("\ue239");
    /// <summary>Official icon: <c>format_color_fill</c></summary>
    public static string? FormatColorFill => Glyph("\ue23a");
    /// <summary>Official icon: <c>format_color_reset</c></summary>
    public static string? FormatColorReset => Glyph("\ue23b");
    /// <summary>Official icon: <c>format_color_text</c></summary>
    public static string? FormatColorText => Glyph("\ue23c");
    /// <summary>Official icon: <c>format_h1</c></summary>
    public static string? FormatH1 => Glyph("\uf85d");
    /// <summary>Official icon: <c>format_h2</c></summary>
    public static string? FormatH2 => Glyph("\uf85e");
    /// <summary>Official icon: <c>format_h3</c></summary>
    public static string? FormatH3 => Glyph("\uf85f");
    /// <summary>Official icon: <c>format_h4</c></summary>
    public static string? FormatH4 => Glyph("\uf860");
    /// <summary>Official icon: <c>format_h5</c></summary>
    public static string? FormatH5 => Glyph("\uf861");
    /// <summary>Official icon: <c>format_h6</c></summary>
    public static string? FormatH6 => Glyph("\uf862");
    /// <summary>Official icon: <c>format_image_back</c></summary>
    public static string? FormatImageBack => Glyph("\ueeb0");
    /// <summary>Official icon: <c>format_image_break_left</c></summary>
    public static string? FormatImageBreakLeft => Glyph("\ueeaf");
    /// <summary>Official icon: <c>format_image_break_right</c></summary>
    public static string? FormatImageBreakRight => Glyph("\ueeae");
    /// <summary>Official icon: <c>format_image_front</c></summary>
    public static string? FormatImageFront => Glyph("\ueead");
    /// <summary>Official icon: <c>format_image_inline_left</c></summary>
    public static string? FormatImageInlineLeft => Glyph("\ueeac");
    /// <summary>Official icon: <c>format_image_inline_right</c></summary>
    public static string? FormatImageInlineRight => Glyph("\U000FFFFD");
    /// <summary>Official icon: <c>format_image_left</c></summary>
    public static string? FormatImageLeft => Glyph("\uf863");
    /// <summary>Official icon: <c>format_image_right</c></summary>
    public static string? FormatImageRight => Glyph("\uf864");
    /// <summary>Official icon: <c>format_indent_decrease</c></summary>
    public static string? FormatIndentDecrease => Glyph("\ue23d");
    /// <summary>Official icon: <c>format_indent_increase</c></summary>
    public static string? FormatIndentIncrease => Glyph("\ue23e");
    /// <summary>Official icon: <c>format_ink_highlighter</c></summary>
    public static string? FormatInkHighlighter => Glyph("\uf82b");
    /// <summary>Official icon: <c>format_italic</c></summary>
    public static string? FormatItalic => Glyph("\ue23f");
    /// <summary>Official icon: <c>format_letter_spacing</c></summary>
    public static string? FormatLetterSpacing => Glyph("\uf773");
    /// <summary>Official icon: <c>format_letter_spacing_2</c></summary>
    public static string? FormatLetterSpacing2 => Glyph("\uf618");
    /// <summary>Official icon: <c>format_letter_spacing_standard</c></summary>
    public static string? FormatLetterSpacingStandard => Glyph("\uf617");
    /// <summary>Official icon: <c>format_letter_spacing_wide</c></summary>
    public static string? FormatLetterSpacingWide => Glyph("\uf616");
    /// <summary>Official icon: <c>format_letter_spacing_wider</c></summary>
    public static string? FormatLetterSpacingWider => Glyph("\uf615");
    /// <summary>Official icon: <c>format_line_spacing</c></summary>
    public static string? FormatLineSpacing => Glyph("\ue240");
    /// <summary>Official icon: <c>format_list_bulleted</c></summary>
    public static string? FormatListBulleted => Glyph("\ue241");
    /// <summary>Official icon: <c>format_list_bulleted_add</c></summary>
    public static string? FormatListBulletedAdd => Glyph("\uf849");
    /// <summary>Official icon: <c>format_list_numbered</c></summary>
    public static string? FormatListNumbered => Glyph("\ue242");
    /// <summary>Official icon: <c>format_list_numbered_rtl</c></summary>
    public static string? FormatListNumberedRtl => Glyph("\ue267");
    /// <summary>Official icon: <c>format_overline</c></summary>
    public static string? FormatOverline => Glyph("\ueb65");
    /// <summary>Official icon: <c>format_paint</c></summary>
    public static string? FormatPaint => Glyph("\ue243");
    /// <summary>Official icon: <c>format_paint_off</c></summary>
    public static string? FormatPaintOff => Glyph("\U000FFF97");
    /// <summary>Official icon: <c>format_paragraph</c></summary>
    public static string? FormatParagraph => Glyph("\uf865");
    /// <summary>Official icon: <c>format_quote</c></summary>
    public static string? FormatQuote => Glyph("\ue244");
    /// <summary>Official icon: <c>format_quote_off</c></summary>
    public static string? FormatQuoteOff => Glyph("\uf413");
    /// <summary>Official icon: <c>format_shapes</c></summary>
    public static string? FormatShapes => Glyph("\ue25e");
    /// <summary>Official icon: <c>format_size</c></summary>
    public static string? FormatSize => Glyph("\ue245");
    /// <summary>Official icon: <c>format_strikethrough</c></summary>
    public static string? FormatStrikethrough => Glyph("\ue246");
    /// <summary>Official icon: <c>format_text_clip</c></summary>
    public static string? FormatTextClip => Glyph("\uf82a");
    /// <summary>Official icon: <c>format_text_overflow</c></summary>
    public static string? FormatTextOverflow => Glyph("\uf829");
    /// <summary>Official icon: <c>format_text_wrap</c></summary>
    public static string? FormatTextWrap => Glyph("\uf828");
    /// <summary>Official icon: <c>format_textdirection_l_to_r</c></summary>
    public static string? FormatTextdirectionLToR => Glyph("\ue247");
    /// <summary>Official icon: <c>format_textdirection_r_to_l</c></summary>
    public static string? FormatTextdirectionRToL => Glyph("\ue248");
    /// <summary>Official icon: <c>format_textdirection_vertical</c></summary>
    public static string? FormatTextdirectionVertical => Glyph("\uf4b8");
    /// <summary>Official icon: <c>format_underlined</c></summary>
    public static string? FormatUnderlined => Glyph("\ue249");
    /// <summary>Official icon: <c>format_underlined_squiggle</c></summary>
    public static string? FormatUnderlinedSquiggle => Glyph("\uf885");
    /// <summary>Official icon: <c>forms_add_on</c></summary>
    public static string? FormsAddOn => Glyph("\uf0c7");
    /// <summary>Official icon: <c>forms_apps_script</c></summary>
    public static string? FormsAppsScript => Glyph("\uf0c8");
    /// <summary>Official icon: <c>fort</c></summary>
    public static string? Fort => Glyph("\ueaad");
    /// <summary>Official icon: <c>forum</c></summary>
    public static string? Forum => Glyph("\ue8af");
    /// <summary>Official icon: <c>forward</c></summary>
    public static string? Forward => Glyph("\uf57a");
    /// <summary>Official icon: <c>forward_10</c></summary>
    public static string? Forward10 => Glyph("\ue056");
    /// <summary>Official icon: <c>forward_30</c></summary>
    public static string? Forward30 => Glyph("\ue057");
    /// <summary>Official icon: <c>forward_5</c></summary>
    public static string? Forward5 => Glyph("\ue058");
    /// <summary>Official icon: <c>forward_circle</c></summary>
    public static string? ForwardCircle => Glyph("\uf6f5");
    /// <summary>Official icon: <c>forward_media</c></summary>
    public static string? ForwardMedia => Glyph("\uf6f4");
    /// <summary>Official icon: <c>forward_to_inbox</c></summary>
    public static string? ForwardToInbox => Glyph("\uf187");
    /// <summary>Official icon: <c>foundation</c></summary>
    public static string? Foundation => Glyph("\uf200");
    /// <summary>Official icon: <c>fragrance</c></summary>
    public static string? Fragrance => Glyph("\uf345");
    /// <summary>Official icon: <c>frame_bug</c></summary>
    public static string? FrameBug => Glyph("\ueeef");
    /// <summary>Official icon: <c>frame_exclamation</c></summary>
    public static string? FrameExclamation => Glyph("\ueeee");
    /// <summary>Official icon: <c>frame_inspect</c></summary>
    public static string? FrameInspect => Glyph("\uf772");
    /// <summary>Official icon: <c>frame_person</c></summary>
    public static string? FramePerson => Glyph("\uf8a6");
    /// <summary>Official icon: <c>frame_person_mic</c></summary>
    public static string? FramePersonMic => Glyph("\uf4d5");
    /// <summary>Official icon: <c>frame_person_off</c></summary>
    public static string? FramePersonOff => Glyph("\uf7d1");
    /// <summary>Official icon: <c>frame_reload</c></summary>
    public static string? FrameReload => Glyph("\uf771");
    /// <summary>Official icon: <c>frame_source</c></summary>
    public static string? FrameSource => Glyph("\uf770");
    /// <summary>Official icon: <c>free_breakfast</c></summary>
    public static string? FreeBreakfast => Glyph("\ueb44");
    /// <summary>Official icon: <c>free_cancellation</c></summary>
    public static string? FreeCancellation => Glyph("\ue748");
    /// <summary>Official icon: <c>front_hand</c></summary>
    public static string? FrontHand => Glyph("\ue769");
    /// <summary>Official icon: <c>front_loader</c></summary>
    public static string? FrontLoader => Glyph("\uf869");
    /// <summary>Official icon: <c>full_coverage</c></summary>
    public static string? FullCoverage => Glyph("\ueb12");
    /// <summary>Official icon: <c>full_hd</c></summary>
    public static string? FullHd => Glyph("\uf58b");
    /// <summary>Official icon: <c>full_stacked_bar_chart</c></summary>
    public static string? FullStackedBarChart => Glyph("\uf212");
    /// <summary>Official icon: <c>fullscreen</c></summary>
    public static string? Fullscreen => Glyph("\ue5d0");
    /// <summary>Official icon: <c>fullscreen_exit</c></summary>
    public static string? FullscreenExit => Glyph("\ue5d1");
    /// <summary>Official icon: <c>fullscreen_portrait</c></summary>
    public static string? FullscreenPortrait => Glyph("\uf45a");
    /// <summary>Official icon: <c>function</c></summary>
    public static string? Function => Glyph("\uf866");
    /// <summary>Official icon: <c>functions</c></summary>
    public static string? Functions => Glyph("\ue24a");
    /// <summary>Official icon: <c>funicular</c></summary>
    public static string? Funicular => Glyph("\uf477");
    /// <summary>Official icon: <c>g_mobiledata</c></summary>
    public static string? GMobiledata => Glyph("\uf010");
    /// <summary>Official icon: <c>g_mobiledata_badge</c></summary>
    public static string? GMobiledataBadge => Glyph("\uf7e1");
    /// <summary>Official icon: <c>g_translate</c></summary>
    public static string? GTranslate => Glyph("\ue927");
    /// <summary>Official icon: <c>gallery_thumbnail</c></summary>
    public static string? GalleryThumbnail => Glyph("\uf86f");
    /// <summary>Official icon: <c>game_bumper_left</c></summary>
    public static string? GameBumperLeft => Glyph("\ueee0");
    /// <summary>Official icon: <c>game_bumper_right</c></summary>
    public static string? GameBumperRight => Glyph("\ueedf");
    /// <summary>Official icon: <c>game_button_l</c></summary>
    public static string? GameButtonL => Glyph("\ueede");
    /// <summary>Official icon: <c>game_button_l1</c></summary>
    public static string? GameButtonL1 => Glyph("\ueedd");
    /// <summary>Official icon: <c>game_button_l2</c></summary>
    public static string? GameButtonL2 => Glyph("\ueedc");
    /// <summary>Official icon: <c>game_button_r</c></summary>
    public static string? GameButtonR => Glyph("\ueedb");
    /// <summary>Official icon: <c>game_button_r1</c></summary>
    public static string? GameButtonR1 => Glyph("\ueeda");
    /// <summary>Official icon: <c>game_button_r2</c></summary>
    public static string? GameButtonR2 => Glyph("\ueed9");
    /// <summary>Official icon: <c>game_button_zl</c></summary>
    public static string? GameButtonZl => Glyph("\ueed8");
    /// <summary>Official icon: <c>game_button_zr</c></summary>
    public static string? GameButtonZr => Glyph("\ueed7");
    /// <summary>Official icon: <c>game_stick_l3</c></summary>
    public static string? GameStickL3 => Glyph("\ueed6");
    /// <summary>Official icon: <c>game_stick_left</c></summary>
    public static string? GameStickLeft => Glyph("\ueed5");
    /// <summary>Official icon: <c>game_stick_r3</c></summary>
    public static string? GameStickR3 => Glyph("\ueed4");
    /// <summary>Official icon: <c>game_stick_right</c></summary>
    public static string? GameStickRight => Glyph("\ueed3");
    /// <summary>Official icon: <c>game_trigger_left</c></summary>
    public static string? GameTriggerLeft => Glyph("\ueed2");
    /// <summary>Official icon: <c>game_trigger_right</c></summary>
    public static string? GameTriggerRight => Glyph("\ueed1");
    /// <summary>Official icon: <c>gamepad</c></summary>
    public static string? Gamepad => Glyph("\ue30f");
    /// <summary>Official icon: <c>gamepad_circle_down</c></summary>
    public static string? GamepadCircleDown => Glyph("\ueed0");
    /// <summary>Official icon: <c>gamepad_circle_left</c></summary>
    public static string? GamepadCircleLeft => Glyph("\ueecf");
    /// <summary>Official icon: <c>gamepad_circle_right</c></summary>
    public static string? GamepadCircleRight => Glyph("\ueece");
    /// <summary>Official icon: <c>gamepad_circle_up</c></summary>
    public static string? GamepadCircleUp => Glyph("\ueecd");
    /// <summary>Official icon: <c>gamepad_down</c></summary>
    public static string? GamepadDown => Glyph("\ueecc");
    /// <summary>Official icon: <c>gamepad_left</c></summary>
    public static string? GamepadLeft => Glyph("\ueecb");
    /// <summary>Official icon: <c>gamepad_right</c></summary>
    public static string? GamepadRight => Glyph("\ueeca");
    /// <summary>Official icon: <c>gamepad_up</c></summary>
    public static string? GamepadUp => Glyph("\ueec9");
    /// <summary>Official icon: <c>games</c></summary>
    public static string? Games => Glyph("\ue30f");
    /// <summary>Official icon: <c>garage</c></summary>
    public static string? Garage => Glyph("\uf011");
    /// <summary>Official icon: <c>garage_check</c></summary>
    public static string? GarageCheck => Glyph("\uf28d");
    /// <summary>Official icon: <c>garage_door</c></summary>
    public static string? GarageDoor => Glyph("\ue714");
    /// <summary>Official icon: <c>garage_door_open</c></summary>
    public static string? GarageDoorOpen => Glyph("\U000FFF77");
    /// <summary>Official icon: <c>garage_home</c></summary>
    public static string? GarageHome => Glyph("\ue82d");
    /// <summary>Official icon: <c>garage_money</c></summary>
    public static string? GarageMoney => Glyph("\uf28c");
    /// <summary>Official icon: <c>garden_cart</c></summary>
    public static string? GardenCart => Glyph("\uf8a9");
    /// <summary>Official icon: <c>gas_meter</c></summary>
    public static string? GasMeter => Glyph("\uec19");
    /// <summary>Official icon: <c>gastroenterology</c></summary>
    public static string? Gastroenterology => Glyph("\ue0f1");
    /// <summary>Official icon: <c>gate</c></summary>
    public static string? Gate => Glyph("\ue277");
    /// <summary>Official icon: <c>gavel</c></summary>
    public static string? Gavel => Glyph("\ue90e");
    /// <summary>Official icon: <c>general_device</c></summary>
    public static string? GeneralDevice => Glyph("\ue6de");
    /// <summary>Official icon: <c>generating_tokens</c></summary>
    public static string? GeneratingTokens => Glyph("\ue749");
    /// <summary>Official icon: <c>genetics</c></summary>
    public static string? Genetics => Glyph("\ue0f3");
    /// <summary>Official icon: <c>genres</c></summary>
    public static string? Genres => Glyph("\ue6ee");
    /// <summary>Official icon: <c>gesture</c></summary>
    public static string? Gesture => Glyph("\ue155");
    /// <summary>Official icon: <c>gesture_select</c></summary>
    public static string? GestureSelect => Glyph("\uf657");
    /// <summary>Official icon: <c>get_app</c></summary>
    public static string? GetApp => Glyph("\uf090");
    /// <summary>Official icon: <c>gif</c></summary>
    public static string? Gif => Glyph("\ue908");
    /// <summary>Official icon: <c>gif_2</c></summary>
    public static string? Gif2 => Glyph("\uf40e");
    /// <summary>Official icon: <c>gif_box</c></summary>
    public static string? GifBox => Glyph("\ue7a3");
    /// <summary>Official icon: <c>girl</c></summary>
    public static string? Girl => Glyph("\ueb68");
    /// <summary>Official icon: <c>gite</c></summary>
    public static string? Gite => Glyph("\ue58b");
    /// <summary>Official icon: <c>glass_cup</c></summary>
    public static string? GlassCup => Glyph("\uf6e3");
    /// <summary>Official icon: <c>globe</c></summary>
    public static string? Globe => Glyph("\ue64c");
    /// <summary>Official icon: <c>globe_2_cancel</c></summary>
    public static string? Globe2Cancel => Glyph("\U000FFFB7");
    /// <summary>Official icon: <c>globe_2_question</c></summary>
    public static string? Globe2Question => Glyph("\U000FFFB6");
    /// <summary>Official icon: <c>globe_asia</c></summary>
    public static string? GlobeAsia => Glyph("\uf799");
    /// <summary>Official icon: <c>globe_book</c></summary>
    public static string? GlobeBook => Glyph("\uf3c9");
    /// <summary>Official icon: <c>globe_clock</c></summary>
    public static string? GlobeClock => Glyph("\U000FFED1");
    /// <summary>Official icon: <c>globe_location_pin</c></summary>
    public static string? GlobeLocationPin => Glyph("\uf35d");
    /// <summary>Official icon: <c>globe_uk</c></summary>
    public static string? GlobeUk => Glyph("\uf798");
    /// <summary>Official icon: <c>glucose</c></summary>
    public static string? Glucose => Glyph("\ue4a0");
    /// <summary>Official icon: <c>glyphs</c></summary>
    public static string? Glyphs => Glyph("\uf8a3");
    /// <summary>Official icon: <c>go_to_line</c></summary>
    public static string? GoToLine => Glyph("\uf71d");
    /// <summary>Official icon: <c>golf_course</c></summary>
    public static string? GolfCourse => Glyph("\ueb45");
    /// <summary>Official icon: <c>gondola_lift</c></summary>
    public static string? GondolaLift => Glyph("\uf476");
    /// <summary>Official icon: <c>google_home_devices</c></summary>
    public static string? GoogleHomeDevices => Glyph("\ue715");
    /// <summary>Official icon: <c>google_plus_reshare</c></summary>
    public static string? GooglePlusReshare => Glyph("\uf57a");
    /// <summary>Official icon: <c>google_tv_remote</c></summary>
    public static string? GoogleTvRemote => Glyph("\uf5db");
    /// <summary>Official icon: <c>google_wifi</c></summary>
    public static string? GoogleWifi => Glyph("\uf579");
    /// <summary>Official icon: <c>gpp_bad</c></summary>
    public static string? GppBad => Glyph("\uf012");
    /// <summary>Official icon: <c>gpp_good</c></summary>
    public static string? GppGood => Glyph("\uf013");
    /// <summary>Official icon: <c>gpp_maybe</c></summary>
    public static string? GppMaybe => Glyph("\uf014");
    /// <summary>Official icon: <c>gps_fixed</c></summary>
    public static string? GpsFixed => Glyph("\ue55c");
    /// <summary>Official icon: <c>gps_not_fixed</c></summary>
    public static string? GpsNotFixed => Glyph("\ue1b7");
    /// <summary>Official icon: <c>gps_off</c></summary>
    public static string? GpsOff => Glyph("\ue1b6");
    /// <summary>Official icon: <c>grade</c></summary>
    public static string? Grade => Glyph("\uf09a");
    /// <summary>Official icon: <c>gradient</c></summary>
    public static string? Gradient => Glyph("\ue3e9");
    /// <summary>Official icon: <c>grading</c></summary>
    public static string? Grading => Glyph("\uea4f");
    /// <summary>Official icon: <c>grain</c></summary>
    public static string? Grain => Glyph("\ue3ea");
    /// <summary>Official icon: <c>graph_1</c></summary>
    public static string? Graph1 => Glyph("\uf3a0");
    /// <summary>Official icon: <c>graph_2</c></summary>
    public static string? Graph2 => Glyph("\uf39f");
    /// <summary>Official icon: <c>graph_3</c></summary>
    public static string? Graph3 => Glyph("\uf39e");
    /// <summary>Official icon: <c>graph_4</c></summary>
    public static string? Graph4 => Glyph("\uf39d");
    /// <summary>Official icon: <c>graph_5</c></summary>
    public static string? Graph5 => Glyph("\uf39c");
    /// <summary>Official icon: <c>graph_6</c></summary>
    public static string? Graph6 => Glyph("\uf39b");
    /// <summary>Official icon: <c>graph_7</c></summary>
    public static string? Graph7 => Glyph("\uf346");
    /// <summary>Official icon: <c>graph_8</c></summary>
    public static string? Graph8 => Glyph("\U000FFFEC");
    /// <summary>Official icon: <c>graphic_eq</c></summary>
    public static string? GraphicEq => Glyph("\ue1b8");
    /// <summary>Official icon: <c>graphic_eq_off</c></summary>
    public static string? GraphicEqOff => Glyph("\U000FFF98");
    /// <summary>Official icon: <c>grass</c></summary>
    public static string? Grass => Glyph("\uf205");
    /// <summary>Official icon: <c>grid_3x3</c></summary>
    public static string? Grid3x3 => Glyph("\uf015");
    /// <summary>Official icon: <c>grid_3x3_off</c></summary>
    public static string? Grid3x3Off => Glyph("\uf67c");
    /// <summary>Official icon: <c>grid_4x4</c></summary>
    public static string? Grid4x4 => Glyph("\uf016");
    /// <summary>Official icon: <c>grid_goldenratio</c></summary>
    public static string? GridGoldenratio => Glyph("\uf017");
    /// <summary>Official icon: <c>grid_guides</c></summary>
    public static string? GridGuides => Glyph("\uf76f");
    /// <summary>Official icon: <c>grid_layout_side</c></summary>
    public static string? GridLayoutSide => Glyph("\U000FFF8D");
    /// <summary>Official icon: <c>grid_off</c></summary>
    public static string? GridOff => Glyph("\ue3eb");
    /// <summary>Official icon: <c>grid_on</c></summary>
    public static string? GridOn => Glyph("\ue3ec");
    /// <summary>Official icon: <c>grid_view</c></summary>
    public static string? GridView => Glyph("\ue9b0");
    /// <summary>Official icon: <c>grocery</c></summary>
    public static string? Grocery => Glyph("\uef97");
    /// <summary>Official icon: <c>group</c></summary>
    public static string? Group => Glyph("\uea21");
    /// <summary>Official icon: <c>group_add</c></summary>
    public static string? GroupAdd => Glyph("\ue7f0");
    /// <summary>Official icon: <c>group_off</c></summary>
    public static string? GroupOff => Glyph("\ue747");
    /// <summary>Official icon: <c>group_remove</c></summary>
    public static string? GroupRemove => Glyph("\ue7ad");
    /// <summary>Official icon: <c>group_search</c></summary>
    public static string? GroupSearch => Glyph("\uf3ce");
    /// <summary>Official icon: <c>group_work</c></summary>
    public static string? GroupWork => Glyph("\ue886");
    /// <summary>Official icon: <c>grouped_bar_chart</c></summary>
    public static string? GroupedBarChart => Glyph("\uf211");
    /// <summary>Official icon: <c>groups</c></summary>
    public static string? Groups => Glyph("\uf233");
    /// <summary>Official icon: <c>groups_2</c></summary>
    public static string? Groups2 => Glyph("\uf8df");
    /// <summary>Official icon: <c>groups_3</c></summary>
    public static string? Groups3 => Glyph("\uf8e0");
    /// <summary>Official icon: <c>guardian</c></summary>
    public static string? Guardian => Glyph("\uf4c1");
    /// <summary>Official icon: <c>gynecology</c></summary>
    public static string? Gynecology => Glyph("\ue0f4");
    /// <summary>Official icon: <c>h_mobiledata</c></summary>
    public static string? HMobiledata => Glyph("\uf018");
    /// <summary>Official icon: <c>h_mobiledata_badge</c></summary>
    public static string? HMobiledataBadge => Glyph("\uf7e0");
    /// <summary>Official icon: <c>h_plus_mobiledata</c></summary>
    public static string? HPlusMobiledata => Glyph("\uf019");
    /// <summary>Official icon: <c>h_plus_mobiledata_badge</c></summary>
    public static string? HPlusMobiledataBadge => Glyph("\uf7df");
    /// <summary>Official icon: <c>hail</c></summary>
    public static string? Hail => Glyph("\ue9b1");
    /// <summary>Official icon: <c>hallway</c></summary>
    public static string? Hallway => Glyph("\ue6f8");
    /// <summary>Official icon: <c>hanami_dango</c></summary>
    public static string? HanamiDango => Glyph("\uf23f");
    /// <summary>Official icon: <c>hand_bones</c></summary>
    public static string? HandBones => Glyph("\uf894");
    /// <summary>Official icon: <c>hand_gesture</c></summary>
    public static string? HandGesture => Glyph("\uef9c");
    /// <summary>Official icon: <c>hand_gesture_off</c></summary>
    public static string? HandGestureOff => Glyph("\uf3f3");
    /// <summary>Official icon: <c>hand_meal</c></summary>
    public static string? HandMeal => Glyph("\uf294");
    /// <summary>Official icon: <c>hand_package</c></summary>
    public static string? HandPackage => Glyph("\uf293");
    /// <summary>Official icon: <c>handheld_controller</c></summary>
    public static string? HandheldController => Glyph("\uf4c6");
    /// <summary>Official icon: <c>handshake</c></summary>
    public static string? Handshake => Glyph("\uebcb");
    /// <summary>Official icon: <c>handwriting_recognition</c></summary>
    public static string? HandwritingRecognition => Glyph("\ueb02");
    /// <summary>Official icon: <c>handyman</c></summary>
    public static string? Handyman => Glyph("\uf10b");
    /// <summary>Official icon: <c>hangout_video</c></summary>
    public static string? HangoutVideo => Glyph("\ue0c1");
    /// <summary>Official icon: <c>hangout_video_off</c></summary>
    public static string? HangoutVideoOff => Glyph("\ue0c2");
    /// <summary>Official icon: <c>hard_disk</c></summary>
    public static string? HardDisk => Glyph("\uf3da");
    /// <summary>Official icon: <c>hard_drive</c></summary>
    public static string? HardDrive => Glyph("\uf80e");
    /// <summary>Official icon: <c>hard_drive_2</c></summary>
    public static string? HardDrive2 => Glyph("\uf7a4");
    /// <summary>Official icon: <c>hardware</c></summary>
    public static string? Hardware => Glyph("\uea59");
    /// <summary>Official icon: <c>hd</c></summary>
    public static string? Hd => Glyph("\ue052");
    /// <summary>Official icon: <c>hdr_auto</c></summary>
    public static string? HdrAuto => Glyph("\uf01a");
    /// <summary>Official icon: <c>hdr_auto_select</c></summary>
    public static string? HdrAutoSelect => Glyph("\uf01b");
    /// <summary>Official icon: <c>hdr_enhanced_select</c></summary>
    public static string? HdrEnhancedSelect => Glyph("\uef51");
    /// <summary>Official icon: <c>hdr_off</c></summary>
    public static string? HdrOff => Glyph("\ue3ed");
    /// <summary>Official icon: <c>hdr_off_select</c></summary>
    public static string? HdrOffSelect => Glyph("\uf01c");
    /// <summary>Official icon: <c>hdr_on</c></summary>
    public static string? HdrOn => Glyph("\ue3ee");
    /// <summary>Official icon: <c>hdr_on_select</c></summary>
    public static string? HdrOnSelect => Glyph("\uf01d");
    /// <summary>Official icon: <c>hdr_plus</c></summary>
    public static string? HdrPlus => Glyph("\uf01e");
    /// <summary>Official icon: <c>hdr_plus_off</c></summary>
    public static string? HdrPlusOff => Glyph("\ue3ef");
    /// <summary>Official icon: <c>hdr_strong</c></summary>
    public static string? HdrStrong => Glyph("\ue3f1");
    /// <summary>Official icon: <c>hdr_weak</c></summary>
    public static string? HdrWeak => Glyph("\ue3f2");
    /// <summary>Official icon: <c>head_mounted_device</c></summary>
    public static string? HeadMountedDevice => Glyph("\uf4c5");
    /// <summary>Official icon: <c>headphones</c></summary>
    public static string? Headphones => Glyph("\uf01f");
    /// <summary>Official icon: <c>headphones_battery</c></summary>
    public static string? HeadphonesBattery => Glyph("\uf020");
    /// <summary>Official icon: <c>headset</c></summary>
    public static string? Headset => Glyph("\uf01f");
    /// <summary>Official icon: <c>headset_mic</c></summary>
    public static string? HeadsetMic => Glyph("\ue311");
    /// <summary>Official icon: <c>headset_off</c></summary>
    public static string? HeadsetOff => Glyph("\ue33a");
    /// <summary>Official icon: <c>healing</c></summary>
    public static string? Healing => Glyph("\ue3f3");
    /// <summary>Official icon: <c>health_and_beauty</c></summary>
    public static string? HealthAndBeauty => Glyph("\uef9d");
    /// <summary>Official icon: <c>health_and_safety</c></summary>
    public static string? HealthAndSafety => Glyph("\ue1d5");
    /// <summary>Official icon: <c>health_cross</c></summary>
    public static string? HealthCross => Glyph("\uf2c3");
    /// <summary>Official icon: <c>health_metrics</c></summary>
    public static string? HealthMetrics => Glyph("\uf6e2");
    /// <summary>Official icon: <c>heap_snapshot_large</c></summary>
    public static string? HeapSnapshotLarge => Glyph("\uf76e");
    /// <summary>Official icon: <c>heap_snapshot_multiple</c></summary>
    public static string? HeapSnapshotMultiple => Glyph("\uf76d");
    /// <summary>Official icon: <c>heap_snapshot_thumbnail</c></summary>
    public static string? HeapSnapshotThumbnail => Glyph("\uf76c");
    /// <summary>Official icon: <c>hearing</c></summary>
    public static string? Hearing => Glyph("\ue023");
    /// <summary>Official icon: <c>hearing_aid</c></summary>
    public static string? HearingAid => Glyph("\uf464");
    /// <summary>Official icon: <c>hearing_aid_disabled</c></summary>
    public static string? HearingAidDisabled => Glyph("\uf3b0");
    /// <summary>Official icon: <c>hearing_aid_disabled_left</c></summary>
    public static string? HearingAidDisabledLeft => Glyph("\uf2ec");
    /// <summary>Official icon: <c>hearing_aid_left</c></summary>
    public static string? HearingAidLeft => Glyph("\uf2ed");
    /// <summary>Official icon: <c>hearing_disabled</c></summary>
    public static string? HearingDisabled => Glyph("\uf104");
    /// <summary>Official icon: <c>heart_broken</c></summary>
    public static string? HeartBroken => Glyph("\ueac2");
    /// <summary>Official icon: <c>heart_check</c></summary>
    public static string? HeartCheck => Glyph("\uf60a");
    /// <summary>Official icon: <c>heart_minus</c></summary>
    public static string? HeartMinus => Glyph("\uf883");
    /// <summary>Official icon: <c>heart_plus</c></summary>
    public static string? HeartPlus => Glyph("\uf884");
    /// <summary>Official icon: <c>heart_smile</c></summary>
    public static string? HeartSmile => Glyph("\uf292");
    /// <summary>Official icon: <c>heat</c></summary>
    public static string? Heat => Glyph("\uf537");
    /// <summary>Official icon: <c>heat_pump</c></summary>
    public static string? HeatPump => Glyph("\uec18");
    /// <summary>Official icon: <c>heat_pump_balance</c></summary>
    public static string? HeatPumpBalance => Glyph("\ue27e");
    /// <summary>Official icon: <c>height</c></summary>
    public static string? Height => Glyph("\uea16");
    /// <summary>Official icon: <c>helicopter</c></summary>
    public static string? Helicopter => Glyph("\uf60c");
    /// <summary>Official icon: <c>help</c></summary>
    public static string? Help => Glyph("\ue8fd");
    /// <summary>Official icon: <c>help_center</c></summary>
    public static string? HelpCenter => Glyph("\uf1c0");
    /// <summary>Official icon: <c>help_clinic</c></summary>
    public static string? HelpClinic => Glyph("\uf810");
    /// <summary>Official icon: <c>help_outline</c></summary>
    public static string? HelpOutline => Glyph("\ue8fd");
    /// <summary>Official icon: <c>hematology</c></summary>
    public static string? Hematology => Glyph("\ue0f6");
    /// <summary>Official icon: <c>hevc</c></summary>
    public static string? Hevc => Glyph("\uf021");
    /// <summary>Official icon: <c>hexagon</c></summary>
    public static string? Hexagon => Glyph("\ueb39");
    /// <summary>Official icon: <c>hide</c></summary>
    public static string? Hide => Glyph("\uef9e");
    /// <summary>Official icon: <c>hide_image</c></summary>
    public static string? HideImage => Glyph("\uf022");
    /// <summary>Official icon: <c>hide_source</c></summary>
    public static string? HideSource => Glyph("\uf023");
    /// <summary>Official icon: <c>high_chair</c></summary>
    public static string? HighChair => Glyph("\uf29a");
    /// <summary>Official icon: <c>high_density</c></summary>
    public static string? HighDensity => Glyph("\uf79c");
    /// <summary>Official icon: <c>high_quality</c></summary>
    public static string? HighQuality => Glyph("\ue024");
    /// <summary>Official icon: <c>high_quality_off</c></summary>
    public static string? HighQualityOff => Glyph("\U000FFED6");
    /// <summary>Official icon: <c>high_res</c></summary>
    public static string? HighRes => Glyph("\uf54b");
    /// <summary>Official icon: <c>highlight</c></summary>
    public static string? Highlight => Glyph("\ue25f");
    /// <summary>Official icon: <c>highlight_alt</c></summary>
    public static string? HighlightAlt => Glyph("\uef52");
    /// <summary>Official icon: <c>highlight_keyboard_focus</c></summary>
    public static string? HighlightKeyboardFocus => Glyph("\uf510");
    /// <summary>Official icon: <c>highlight_mouse_cursor</c></summary>
    public static string? HighlightMouseCursor => Glyph("\uf511");
    /// <summary>Official icon: <c>highlight_off</c></summary>
    public static string? HighlightOff => Glyph("\ue888");
    /// <summary>Official icon: <c>highlight_text_cursor</c></summary>
    public static string? HighlightTextCursor => Glyph("\uf512");
    /// <summary>Official icon: <c>highlighter_size_1</c></summary>
    public static string? HighlighterSize1 => Glyph("\uf76b");
    /// <summary>Official icon: <c>highlighter_size_2</c></summary>
    public static string? HighlighterSize2 => Glyph("\uf76a");
    /// <summary>Official icon: <c>highlighter_size_3</c></summary>
    public static string? HighlighterSize3 => Glyph("\uf769");
    /// <summary>Official icon: <c>highlighter_size_4</c></summary>
    public static string? HighlighterSize4 => Glyph("\uf768");
    /// <summary>Official icon: <c>highlighter_size_5</c></summary>
    public static string? HighlighterSize5 => Glyph("\uf767");
    /// <summary>Official icon: <c>hiking</c></summary>
    public static string? Hiking => Glyph("\ue50a");
    /// <summary>Official icon: <c>history</c></summary>
    public static string? History => Glyph("\ue8b3");
    /// <summary>Official icon: <c>history_2</c></summary>
    public static string? History2 => Glyph("\uf3e6");
    /// <summary>Official icon: <c>history_edu</c></summary>
    public static string? HistoryEdu => Glyph("\uea3e");
    /// <summary>Official icon: <c>history_off</c></summary>
    public static string? HistoryOff => Glyph("\uf4da");
    /// <summary>Official icon: <c>history_toggle_off</c></summary>
    public static string? HistoryToggleOff => Glyph("\uf17d");
    /// <summary>Official icon: <c>hive</c></summary>
    public static string? Hive => Glyph("\ueaa6");
    /// <summary>Official icon: <c>hls</c></summary>
    public static string? Hls => Glyph("\ueb8a");
    /// <summary>Official icon: <c>hls_off</c></summary>
    public static string? HlsOff => Glyph("\ueb8c");
    /// <summary>Official icon: <c>holiday_village</c></summary>
    public static string? HolidayVillage => Glyph("\ue58a");
    /// <summary>Official icon: <c>home</c></summary>
    public static string? Home => Glyph("\ue9b2");
    /// <summary>Official icon: <c>home_and_garden</c></summary>
    public static string? HomeAndGarden => Glyph("\uef9f");
    /// <summary>Official icon: <c>home_app_logo</c></summary>
    public static string? HomeAppLogo => Glyph("\ue295");
    /// <summary>Official icon: <c>home_filled</c></summary>
    public static string? HomeFilled => Glyph("\ue9b2");
    /// <summary>Official icon: <c>home_health</c></summary>
    public static string? HomeHealth => Glyph("\ue4b9");
    /// <summary>Official icon: <c>home_improvement_and_tools</c></summary>
    public static string? HomeImprovementAndTools => Glyph("\uefa0");
    /// <summary>Official icon: <c>home_iot_device</c></summary>
    public static string? HomeIotDevice => Glyph("\ue283");
    /// <summary>Official icon: <c>home_max</c></summary>
    public static string? HomeMax => Glyph("\uf024");
    /// <summary>Official icon: <c>home_max_dots</c></summary>
    public static string? HomeMaxDots => Glyph("\ue849");
    /// <summary>Official icon: <c>home_mini</c></summary>
    public static string? HomeMini => Glyph("\uf025");
    /// <summary>Official icon: <c>home_pin</c></summary>
    public static string? HomePin => Glyph("\uf14d");
    /// <summary>Official icon: <c>home_repair_service</c></summary>
    public static string? HomeRepairService => Glyph("\uf100");
    /// <summary>Official icon: <c>home_speaker</c></summary>
    public static string? HomeSpeaker => Glyph("\uf11c");
    /// <summary>Official icon: <c>home_storage</c></summary>
    public static string? HomeStorage => Glyph("\uf86c");
    /// <summary>Official icon: <c>home_storage_gear</c></summary>
    public static string? HomeStorageGear => Glyph("\U000FFF7E");
    /// <summary>Official icon: <c>home_work</c></summary>
    public static string? HomeWork => Glyph("\uf030");
    /// <summary>Official icon: <c>horizontal_align_center</c></summary>
    public static string? HorizontalAlignCenter => Glyph("\U000FFF9C");
    /// <summary>Official icon: <c>horizontal_align_left</c></summary>
    public static string? HorizontalAlignLeft => Glyph("\U000FFF9B");
    /// <summary>Official icon: <c>horizontal_align_right</c></summary>
    public static string? HorizontalAlignRight => Glyph("\U000FFF9A");
    /// <summary>Official icon: <c>horizontal_distribute</c></summary>
    public static string? HorizontalDistribute => Glyph("\ue014");
    /// <summary>Official icon: <c>horizontal_rule</c></summary>
    public static string? HorizontalRule => Glyph("\uf108");
    /// <summary>Official icon: <c>horizontal_split</c></summary>
    public static string? HorizontalSplit => Glyph("\ue947");
    /// <summary>Official icon: <c>host</c></summary>
    public static string? Host => Glyph("\uf3d9");
    /// <summary>Official icon: <c>hot_tub</c></summary>
    public static string? HotTub => Glyph("\ueb46");
    /// <summary>Official icon: <c>hotel</c></summary>
    public static string? Hotel => Glyph("\ue549");
    /// <summary>Official icon: <c>hotel_class</c></summary>
    public static string? HotelClass => Glyph("\ue743");
    /// <summary>Official icon: <c>hourglass</c></summary>
    public static string? Hourglass => Glyph("\uebff");
    /// <summary>Official icon: <c>hourglass_arrow_down</c></summary>
    public static string? HourglassArrowDown => Glyph("\uf37e");
    /// <summary>Official icon: <c>hourglass_arrow_up</c></summary>
    public static string? HourglassArrowUp => Glyph("\uf37d");
    /// <summary>Official icon: <c>hourglass_bottom</c></summary>
    public static string? HourglassBottom => Glyph("\uea5c");
    /// <summary>Official icon: <c>hourglass_check</c></summary>
    public static string? HourglassCheck => Glyph("\U000FFFED");
    /// <summary>Official icon: <c>hourglass_disabled</c></summary>
    public static string? HourglassDisabled => Glyph("\uef53");
    /// <summary>Official icon: <c>hourglass_empty</c></summary>
    public static string? HourglassEmpty => Glyph("\ue88b");
    /// <summary>Official icon: <c>hourglass_full</c></summary>
    public static string? HourglassFull => Glyph("\ue88c");
    /// <summary>Official icon: <c>hourglass_pause</c></summary>
    public static string? HourglassPause => Glyph("\uf38c");
    /// <summary>Official icon: <c>hourglass_top</c></summary>
    public static string? HourglassTop => Glyph("\uea5b");
    /// <summary>Official icon: <c>house</c></summary>
    public static string? House => Glyph("\uea44");
    /// <summary>Official icon: <c>house_siding</c></summary>
    public static string? HouseSiding => Glyph("\uf202");
    /// <summary>Official icon: <c>house_with_shield</c></summary>
    public static string? HouseWithShield => Glyph("\ue786");
    /// <summary>Official icon: <c>houseboat</c></summary>
    public static string? Houseboat => Glyph("\ue584");
    /// <summary>Official icon: <c>household_supplies</c></summary>
    public static string? HouseholdSupplies => Glyph("\uefa1");
    /// <summary>Official icon: <c>hov</c></summary>
    public static string? Hov => Glyph("\uf475");
    /// <summary>Official icon: <c>how_to_reg</c></summary>
    public static string? HowToReg => Glyph("\ue174");
    /// <summary>Official icon: <c>how_to_vote</c></summary>
    public static string? HowToVote => Glyph("\ue175");
    /// <summary>Official icon: <c>hr_resting</c></summary>
    public static string? HrResting => Glyph("\uf6ba");
    /// <summary>Official icon: <c>html</c></summary>
    public static string? Html => Glyph("\ueb7e");
    /// <summary>Official icon: <c>http</c></summary>
    public static string? Http => Glyph("\ue902");
    /// <summary>Official icon: <c>https</c></summary>
    public static string? Https => Glyph("\ue899");
    /// <summary>Official icon: <c>hub</c></summary>
    public static string? Hub => Glyph("\ue9f4");
    /// <summary>Official icon: <c>humerus</c></summary>
    public static string? Humerus => Glyph("\uf895");
    /// <summary>Official icon: <c>humerus_alt</c></summary>
    public static string? HumerusAlt => Glyph("\uf896");
    /// <summary>Official icon: <c>humidity_high</c></summary>
    public static string? HumidityHigh => Glyph("\uf163");
    /// <summary>Official icon: <c>humidity_indoor</c></summary>
    public static string? HumidityIndoor => Glyph("\uf558");
    /// <summary>Official icon: <c>humidity_low</c></summary>
    public static string? HumidityLow => Glyph("\uf164");
    /// <summary>Official icon: <c>humidity_mid</c></summary>
    public static string? HumidityMid => Glyph("\uf165");
    /// <summary>Official icon: <c>humidity_percentage</c></summary>
    public static string? HumidityPercentage => Glyph("\uf87e");
    /// <summary>Official icon: <c>hvac</c></summary>
    public static string? Hvac => Glyph("\uf10e");
    /// <summary>Official icon: <c>hvac_max_defrost</c></summary>
    public static string? HvacMaxDefrost => Glyph("\uf332");
    /// <summary>Official icon: <c>ice_skating</c></summary>
    public static string? IceSkating => Glyph("\ue50b");
    /// <summary>Official icon: <c>icecream</c></summary>
    public static string? Icecream => Glyph("\uea69");
    /// <summary>Official icon: <c>id_card</c></summary>
    public static string? IdCard => Glyph("\uf4ca");
    /// <summary>Official icon: <c>id_card_2</c></summary>
    public static string? IdCard2 => Glyph("\U000FFEEA");
    /// <summary>Official icon: <c>identity_aware_proxy</c></summary>
    public static string? IdentityAwareProxy => Glyph("\ue2dd");
    /// <summary>Official icon: <c>identity_platform</c></summary>
    public static string? IdentityPlatform => Glyph("\uebb7");
    /// <summary>Official icon: <c>ifl</c></summary>
    public static string? Ifl => Glyph("\ue025");
    /// <summary>Official icon: <c>iframe</c></summary>
    public static string? Iframe => Glyph("\uf71b");
    /// <summary>Official icon: <c>iframe_off</c></summary>
    public static string? IframeOff => Glyph("\uf71c");
    /// <summary>Official icon: <c>image</c></summary>
    public static string? Image => Glyph("\ue3f4");
    /// <summary>Official icon: <c>image_arrow_up</c></summary>
    public static string? ImageArrowUp => Glyph("\uf317");
    /// <summary>Official icon: <c>image_aspect_ratio</c></summary>
    public static string? ImageAspectRatio => Glyph("\ue6a6");
    /// <summary>Official icon: <c>image_inset</c></summary>
    public static string? ImageInset => Glyph("\uf247");
    /// <summary>Official icon: <c>image_not_supported</c></summary>
    public static string? ImageNotSupported => Glyph("\uf116");
    /// <summary>Official icon: <c>image_search</c></summary>
    public static string? ImageSearch => Glyph("\ue43f");
    /// <summary>Official icon: <c>imagesearch_roller</c></summary>
    public static string? ImagesearchRoller => Glyph("\ue9b4");
    /// <summary>Official icon: <c>imagesmode</c></summary>
    public static string? Imagesmode => Glyph("\uefa2");
    /// <summary>Official icon: <c>immunology</c></summary>
    public static string? Immunology => Glyph("\ue0fb");
    /// <summary>Official icon: <c>import_contacts</c></summary>
    public static string? ImportContacts => Glyph("\ue0e0");
    /// <summary>Official icon: <c>import_export</c></summary>
    public static string? ImportExport => Glyph("\ue8d5");
    /// <summary>Official icon: <c>important_devices</c></summary>
    public static string? ImportantDevices => Glyph("\ue912");
    /// <summary>Official icon: <c>in_home_mode</c></summary>
    public static string? InHomeMode => Glyph("\ue833");
    /// <summary>Official icon: <c>inactive_order</c></summary>
    public static string? InactiveOrder => Glyph("\ue0fc");
    /// <summary>Official icon: <c>inbox</c></summary>
    public static string? Inbox => Glyph("\ue156");
    /// <summary>Official icon: <c>inbox_customize</c></summary>
    public static string? InboxCustomize => Glyph("\uf859");
    /// <summary>Official icon: <c>inbox_text</c></summary>
    public static string? InboxText => Glyph("\uf399");
    /// <summary>Official icon: <c>inbox_text_asterisk</c></summary>
    public static string? InboxTextAsterisk => Glyph("\uf360");
    /// <summary>Official icon: <c>inbox_text_person</c></summary>
    public static string? InboxTextPerson => Glyph("\uf35e");
    /// <summary>Official icon: <c>inbox_text_share</c></summary>
    public static string? InboxTextShare => Glyph("\uf35c");
    /// <summary>Official icon: <c>incomplete_circle</c></summary>
    public static string? IncompleteCircle => Glyph("\ue79b");
    /// <summary>Official icon: <c>indeterminate_check_box</c></summary>
    public static string? IndeterminateCheckBox => Glyph("\ue909");
    /// <summary>Official icon: <c>indeterminate_question_box</c></summary>
    public static string? IndeterminateQuestionBox => Glyph("\uf56d");
    /// <summary>Official icon: <c>info</c></summary>
    public static string? Info => Glyph("\ue88e");
    /// <summary>Official icon: <c>info_i</c></summary>
    public static string? InfoI => Glyph("\uf59b");
    /// <summary>Official icon: <c>infrared</c></summary>
    public static string? Infrared => Glyph("\uf87c");
    /// <summary>Official icon: <c>ink_eraser</c></summary>
    public static string? InkEraser => Glyph("\ue6d0");
    /// <summary>Official icon: <c>ink_eraser_off</c></summary>
    public static string? InkEraserOff => Glyph("\ue7e3");
    /// <summary>Official icon: <c>ink_highlighter</c></summary>
    public static string? InkHighlighter => Glyph("\ue6d1");
    /// <summary>Official icon: <c>ink_highlighter_move</c></summary>
    public static string? InkHighlighterMove => Glyph("\uf524");
    /// <summary>Official icon: <c>ink_highlighter_off</c></summary>
    public static string? InkHighlighterOff => Glyph("\U000FFF14");
    /// <summary>Official icon: <c>ink_marker</c></summary>
    public static string? InkMarker => Glyph("\ue6d2");
    /// <summary>Official icon: <c>ink_pen</c></summary>
    public static string? InkPen => Glyph("\ue6d3");
    /// <summary>Official icon: <c>ink_selection</c></summary>
    public static string? InkSelection => Glyph("\uef52");
    /// <summary>Official icon: <c>inpatient</c></summary>
    public static string? Inpatient => Glyph("\ue0fe");
    /// <summary>Official icon: <c>input</c></summary>
    public static string? Input => Glyph("\ue890");
    /// <summary>Official icon: <c>input_circle</c></summary>
    public static string? InputCircle => Glyph("\uf71a");
    /// <summary>Official icon: <c>insert_chart</c></summary>
    public static string? InsertChart => Glyph("\uf0cc");
    /// <summary>Official icon: <c>insert_chart_filled</c></summary>
    public static string? InsertChartFilled => Glyph("\uf0cc");
    /// <summary>Official icon: <c>insert_chart_outlined</c></summary>
    public static string? InsertChartOutlined => Glyph("\uf0cc");
    /// <summary>Official icon: <c>insert_comment</c></summary>
    public static string? InsertComment => Glyph("\ue24c");
    /// <summary>Official icon: <c>insert_drive_file</c></summary>
    public static string? InsertDriveFile => Glyph("\ue66d");
    /// <summary>Official icon: <c>insert_emoticon</c></summary>
    public static string? InsertEmoticon => Glyph("\uea22");
    /// <summary>Official icon: <c>insert_invitation</c></summary>
    public static string? InsertInvitation => Glyph("\ue878");
    /// <summary>Official icon: <c>insert_link</c></summary>
    public static string? InsertLink => Glyph("\ue250");
    /// <summary>Official icon: <c>insert_page_break</c></summary>
    public static string? InsertPageBreak => Glyph("\ueaca");
    /// <summary>Official icon: <c>insert_photo</c></summary>
    public static string? InsertPhoto => Glyph("\ue3f4");
    /// <summary>Official icon: <c>insert_text</c></summary>
    public static string? InsertText => Glyph("\uf827");
    /// <summary>Official icon: <c>insights</c></summary>
    public static string? Insights => Glyph("\uf092");
    /// <summary>Official icon: <c>install_desktop</c></summary>
    public static string? InstallDesktop => Glyph("\ueb71");
    /// <summary>Official icon: <c>install_mobile</c></summary>
    public static string? InstallMobile => Glyph("\uf2cd");
    /// <summary>Official icon: <c>instant_mix</c></summary>
    public static string? InstantMix => Glyph("\ue026");
    /// <summary>Official icon: <c>integration_instructions</c></summary>
    public static string? IntegrationInstructions => Glyph("\uef54");
    /// <summary>Official icon: <c>interactive_space</c></summary>
    public static string? InteractiveSpace => Glyph("\uf7ff");
    /// <summary>Official icon: <c>interests</c></summary>
    public static string? Interests => Glyph("\ue7c8");
    /// <summary>Official icon: <c>interpreter_mode</c></summary>
    public static string? InterpreterMode => Glyph("\ue83b");
    /// <summary>Official icon: <c>inventory</c></summary>
    public static string? Inventory => Glyph("\ue179");
    /// <summary>Official icon: <c>inventory_2</c></summary>
    public static string? Inventory2 => Glyph("\ue1a1");
    /// <summary>Official icon: <c>invert_colors</c></summary>
    public static string? InvertColors => Glyph("\ue891");
    /// <summary>Official icon: <c>invert_colors_off</c></summary>
    public static string? InvertColorsOff => Glyph("\ue0c4");
    /// <summary>Official icon: <c>ios</c></summary>
    public static string? Ios => Glyph("\ue027");
    /// <summary>Official icon: <c>ios_share</c></summary>
    public static string? IosShare => Glyph("\ue6b8");
    /// <summary>Official icon: <c>iron</c></summary>
    public static string? Iron => Glyph("\ue583");
    /// <summary>Official icon: <c>iso</c></summary>
    public static string? Iso => Glyph("\ue3f6");
    /// <summary>Official icon: <c>jamboard_kiosk</c></summary>
    public static string? JamboardKiosk => Glyph("\ue9b5");
    /// <summary>Official icon: <c>japanese_curry</c></summary>
    public static string? JapaneseCurry => Glyph("\uf284");
    /// <summary>Official icon: <c>japanese_flag</c></summary>
    public static string? JapaneseFlag => Glyph("\uf283");
    /// <summary>Official icon: <c>javascript</c></summary>
    public static string? Javascript => Glyph("\ueb7c");
    /// <summary>Official icon: <c>jewelry</c></summary>
    public static string? Jewelry => Glyph("\U000FFEDB");
    /// <summary>Official icon: <c>join</c></summary>
    public static string? Join => Glyph("\uf84f");
    /// <summary>Official icon: <c>join_full</c></summary>
    public static string? JoinFull => Glyph("\uf84f");
    /// <summary>Official icon: <c>join_inner</c></summary>
    public static string? JoinInner => Glyph("\ueaf4");
    /// <summary>Official icon: <c>join_left</c></summary>
    public static string? JoinLeft => Glyph("\ueaf2");
    /// <summary>Official icon: <c>join_right</c></summary>
    public static string? JoinRight => Glyph("\ueaea");
    /// <summary>Official icon: <c>joystick</c></summary>
    public static string? Joystick => Glyph("\uf5ee");
    /// <summary>Official icon: <c>jump_to_element</c></summary>
    public static string? JumpToElement => Glyph("\uf719");
    /// <summary>Official icon: <c>kanji_alcohol</c></summary>
    public static string? KanjiAlcohol => Glyph("\uf23e");
    /// <summary>Official icon: <c>kayaking</c></summary>
    public static string? Kayaking => Glyph("\ue50c");
    /// <summary>Official icon: <c>kebab_dining</c></summary>
    public static string? KebabDining => Glyph("\ue842");
    /// <summary>Official icon: <c>keep</c></summary>
    public static string? Keep => Glyph("\uf027");
    /// <summary>Official icon: <c>keep_off</c></summary>
    public static string? KeepOff => Glyph("\ue6f9");
    /// <summary>Official icon: <c>keep_pin</c></summary>
    public static string? KeepPin => Glyph("\uf027");
    /// <summary>Official icon: <c>keep_public</c></summary>
    public static string? KeepPublic => Glyph("\uf56f");
    /// <summary>Official icon: <c>kettle</c></summary>
    public static string? Kettle => Glyph("\ue2b9");
    /// <summary>Official icon: <c>key</c></summary>
    public static string? Key => Glyph("\ue73c");
    /// <summary>Official icon: <c>key_off</c></summary>
    public static string? KeyOff => Glyph("\ueb84");
    /// <summary>Official icon: <c>key_vertical</c></summary>
    public static string? KeyVertical => Glyph("\uf51a");
    /// <summary>Official icon: <c>key_visualizer</c></summary>
    public static string? KeyVisualizer => Glyph("\uf199");
    /// <summary>Official icon: <c>keyboard</c></summary>
    public static string? Keyboard => Glyph("\ue312");
    /// <summary>Official icon: <c>keyboard_alt</c></summary>
    public static string? KeyboardAlt => Glyph("\uf028");
    /// <summary>Official icon: <c>keyboard_arrow_down</c></summary>
    public static string? KeyboardArrowDown => Glyph("\ue313");
    /// <summary>Official icon: <c>keyboard_arrow_left</c></summary>
    public static string? KeyboardArrowLeft => Glyph("\ue314");
    /// <summary>Official icon: <c>keyboard_arrow_right</c></summary>
    public static string? KeyboardArrowRight => Glyph("\ue315");
    /// <summary>Official icon: <c>keyboard_arrow_up</c></summary>
    public static string? KeyboardArrowUp => Glyph("\ue316");
    /// <summary>Official icon: <c>keyboard_backspace</c></summary>
    public static string? KeyboardBackspace => Glyph("\ue317");
    /// <summary>Official icon: <c>keyboard_capslock</c></summary>
    public static string? KeyboardCapslock => Glyph("\ue318");
    /// <summary>Official icon: <c>keyboard_capslock_badge</c></summary>
    public static string? KeyboardCapslockBadge => Glyph("\uf7de");
    /// <summary>Official icon: <c>keyboard_command_key</c></summary>
    public static string? KeyboardCommandKey => Glyph("\ueae7");
    /// <summary>Official icon: <c>keyboard_control_key</c></summary>
    public static string? KeyboardControlKey => Glyph("\ueae6");
    /// <summary>Official icon: <c>keyboard_double_arrow_down</c></summary>
    public static string? KeyboardDoubleArrowDown => Glyph("\uead0");
    /// <summary>Official icon: <c>keyboard_double_arrow_left</c></summary>
    public static string? KeyboardDoubleArrowLeft => Glyph("\ueac3");
    /// <summary>Official icon: <c>keyboard_double_arrow_right</c></summary>
    public static string? KeyboardDoubleArrowRight => Glyph("\ueac9");
    /// <summary>Official icon: <c>keyboard_double_arrow_up</c></summary>
    public static string? KeyboardDoubleArrowUp => Glyph("\ueacf");
    /// <summary>Official icon: <c>keyboard_external_input</c></summary>
    public static string? KeyboardExternalInput => Glyph("\uf7dd");
    /// <summary>Official icon: <c>keyboard_full</c></summary>
    public static string? KeyboardFull => Glyph("\uf7dc");
    /// <summary>Official icon: <c>keyboard_hide</c></summary>
    public static string? KeyboardHide => Glyph("\ue31a");
    /// <summary>Official icon: <c>keyboard_keys</c></summary>
    public static string? KeyboardKeys => Glyph("\uf67b");
    /// <summary>Official icon: <c>keyboard_lock</c></summary>
    public static string? KeyboardLock => Glyph("\uf492");
    /// <summary>Official icon: <c>keyboard_lock_off</c></summary>
    public static string? KeyboardLockOff => Glyph("\uf491");
    /// <summary>Official icon: <c>keyboard_off</c></summary>
    public static string? KeyboardOff => Glyph("\uf67a");
    /// <summary>Official icon: <c>keyboard_onscreen</c></summary>
    public static string? KeyboardOnscreen => Glyph("\uf7db");
    /// <summary>Official icon: <c>keyboard_option_key</c></summary>
    public static string? KeyboardOptionKey => Glyph("\ueae8");
    /// <summary>Official icon: <c>keyboard_previous_language</c></summary>
    public static string? KeyboardPreviousLanguage => Glyph("\uf7da");
    /// <summary>Official icon: <c>keyboard_return</c></summary>
    public static string? KeyboardReturn => Glyph("\ue31b");
    /// <summary>Official icon: <c>keyboard_tab</c></summary>
    public static string? KeyboardTab => Glyph("\ue31c");
    /// <summary>Official icon: <c>keyboard_tab_rtl</c></summary>
    public static string? KeyboardTabRtl => Glyph("\uec73");
    /// <summary>Official icon: <c>keyboard_voice</c></summary>
    public static string? KeyboardVoice => Glyph("\ue31d");
    /// <summary>Official icon: <c>kid_star</c></summary>
    public static string? KidStar => Glyph("\uf526");
    /// <summary>Official icon: <c>king_bed</c></summary>
    public static string? KingBed => Glyph("\uea45");
    /// <summary>Official icon: <c>kitchen</c></summary>
    public static string? Kitchen => Glyph("\ueb47");
    /// <summary>Official icon: <c>kitesurfing</c></summary>
    public static string? Kitesurfing => Glyph("\ue50d");
    /// <summary>Official icon: <c>lab_panel</c></summary>
    public static string? LabPanel => Glyph("\ue103");
    /// <summary>Official icon: <c>lab_profile</c></summary>
    public static string? LabProfile => Glyph("\ue104");
    /// <summary>Official icon: <c>lab_research</c></summary>
    public static string? LabResearch => Glyph("\uf80b");
    /// <summary>Official icon: <c>label</c></summary>
    public static string? Label => Glyph("\ue893");
    /// <summary>Official icon: <c>label_important</c></summary>
    public static string? LabelImportant => Glyph("\ue948");
    /// <summary>Official icon: <c>label_important_outline</c></summary>
    public static string? LabelImportantOutline => Glyph("\ue948");
    /// <summary>Official icon: <c>label_off</c></summary>
    public static string? LabelOff => Glyph("\ue9b6");
    /// <summary>Official icon: <c>label_outline</c></summary>
    public static string? LabelOutline => Glyph("\ue893");
    /// <summary>Official icon: <c>labs</c></summary>
    public static string? Labs => Glyph("\ue105");
    /// <summary>Official icon: <c>lan</c></summary>
    public static string? Lan => Glyph("\ueb2f");
    /// <summary>Official icon: <c>landscape</c></summary>
    public static string? Landscape => Glyph("\ue564");
    /// <summary>Official icon: <c>landscape_2</c></summary>
    public static string? Landscape2 => Glyph("\uf4c4");
    /// <summary>Official icon: <c>landscape_2_edit</c></summary>
    public static string? Landscape2Edit => Glyph("\uf310");
    /// <summary>Official icon: <c>landscape_2_off</c></summary>
    public static string? Landscape2Off => Glyph("\uf4c3");
    /// <summary>Official icon: <c>landslide</c></summary>
    public static string? Landslide => Glyph("\uebd7");
    /// <summary>Official icon: <c>language</c></summary>
    public static string? Language => Glyph("\uea07");
    /// <summary>Official icon: <c>language_chinese_array</c></summary>
    public static string? LanguageChineseArray => Glyph("\uf766");
    /// <summary>Official icon: <c>language_chinese_cangjie</c></summary>
    public static string? LanguageChineseCangjie => Glyph("\uf765");
    /// <summary>Official icon: <c>language_chinese_dayi</c></summary>
    public static string? LanguageChineseDayi => Glyph("\uf764");
    /// <summary>Official icon: <c>language_chinese_pinyin</c></summary>
    public static string? LanguageChinesePinyin => Glyph("\uf763");
    /// <summary>Official icon: <c>language_chinese_quick</c></summary>
    public static string? LanguageChineseQuick => Glyph("\uf762");
    /// <summary>Official icon: <c>language_chinese_wubi</c></summary>
    public static string? LanguageChineseWubi => Glyph("\uf761");
    /// <summary>Official icon: <c>language_french</c></summary>
    public static string? LanguageFrench => Glyph("\uf760");
    /// <summary>Official icon: <c>language_gb_english</c></summary>
    public static string? LanguageGbEnglish => Glyph("\uf75f");
    /// <summary>Official icon: <c>language_international</c></summary>
    public static string? LanguageInternational => Glyph("\uf75e");
    /// <summary>Official icon: <c>language_japanese_kana</c></summary>
    public static string? LanguageJapaneseKana => Glyph("\uf513");
    /// <summary>Official icon: <c>language_korean_latin</c></summary>
    public static string? LanguageKoreanLatin => Glyph("\uf75d");
    /// <summary>Official icon: <c>language_pinyin</c></summary>
    public static string? LanguagePinyin => Glyph("\uf75c");
    /// <summary>Official icon: <c>language_spanish</c></summary>
    public static string? LanguageSpanish => Glyph("\uf5e9");
    /// <summary>Official icon: <c>language_us</c></summary>
    public static string? LanguageUs => Glyph("\uf759");
    /// <summary>Official icon: <c>language_us_colemak</c></summary>
    public static string? LanguageUsColemak => Glyph("\uf75b");
    /// <summary>Official icon: <c>language_us_dvorak</c></summary>
    public static string? LanguageUsDvorak => Glyph("\uf75a");
    /// <summary>Official icon: <c>laps</c></summary>
    public static string? Laps => Glyph("\uf6b9");
    /// <summary>Official icon: <c>laptop</c></summary>
    public static string? Laptop => Glyph("\ue31e");
    /// <summary>Official icon: <c>laptop_car</c></summary>
    public static string? LaptopCar => Glyph("\uf3cd");
    /// <summary>Official icon: <c>laptop_chromebook</c></summary>
    public static string? LaptopChromebook => Glyph("\ue31f");
    /// <summary>Official icon: <c>laptop_mac</c></summary>
    public static string? LaptopMac => Glyph("\ue320");
    /// <summary>Official icon: <c>laptop_windows</c></summary>
    public static string? LaptopWindows => Glyph("\ue321");
    /// <summary>Official icon: <c>lasso_select</c></summary>
    public static string? LassoSelect => Glyph("\ueb03");
    /// <summary>Official icon: <c>last_page</c></summary>
    public static string? LastPage => Glyph("\ue5dd");
    /// <summary>Official icon: <c>launch</c></summary>
    public static string? Launch => Glyph("\ue89e");
    /// <summary>Official icon: <c>laundry</c></summary>
    public static string? Laundry => Glyph("\ue2a8");
    /// <summary>Official icon: <c>layers</c></summary>
    public static string? Layers => Glyph("\ue53b");
    /// <summary>Official icon: <c>layers_clear</c></summary>
    public static string? LayersClear => Glyph("\ue53c");
    /// <summary>Official icon: <c>lda</c></summary>
    public static string? Lda => Glyph("\ue106");
    /// <summary>Official icon: <c>leaderboard</c></summary>
    public static string? Leaderboard => Glyph("\uf20c");
    /// <summary>Official icon: <c>leak_add</c></summary>
    public static string? LeakAdd => Glyph("\ue3f8");
    /// <summary>Official icon: <c>leak_remove</c></summary>
    public static string? LeakRemove => Glyph("\ue3f9");
    /// <summary>Official icon: <c>left_click</c></summary>
    public static string? LeftClick => Glyph("\uf718");
    /// <summary>Official icon: <c>left_panel_close</c></summary>
    public static string? LeftPanelClose => Glyph("\uf717");
    /// <summary>Official icon: <c>left_panel_open</c></summary>
    public static string? LeftPanelOpen => Glyph("\uf716");
    /// <summary>Official icon: <c>legend_toggle</c></summary>
    public static string? LegendToggle => Glyph("\uf11b");
    /// <summary>Official icon: <c>lens</c></summary>
    public static string? Lens => Glyph("\ue3fa");
    /// <summary>Official icon: <c>lens_blur</c></summary>
    public static string? LensBlur => Glyph("\uf029");
    /// <summary>Official icon: <c>letter_switch</c></summary>
    public static string? LetterSwitch => Glyph("\uf758");
    /// <summary>Official icon: <c>library_add</c></summary>
    public static string? LibraryAdd => Glyph("\ue03c");
    /// <summary>Official icon: <c>library_add_check</c></summary>
    public static string? LibraryAddCheck => Glyph("\ue9b7");
    /// <summary>Official icon: <c>library_books</c></summary>
    public static string? LibraryBooks => Glyph("\ue02f");
    /// <summary>Official icon: <c>library_music</c></summary>
    public static string? LibraryMusic => Glyph("\ue030");
    /// <summary>Official icon: <c>license</c></summary>
    public static string? License => Glyph("\ueb04");
    /// <summary>Official icon: <c>lift_to_talk</c></summary>
    public static string? LiftToTalk => Glyph("\uefa3");
    /// <summary>Official icon: <c>light</c></summary>
    public static string? Light => Glyph("\uf02a");
    /// <summary>Official icon: <c>light_group</c></summary>
    public static string? LightGroup => Glyph("\ue28b");
    /// <summary>Official icon: <c>light_group_2</c></summary>
    public static string? LightGroup2 => Glyph("\U000FFF76");
    /// <summary>Official icon: <c>light_mode</c></summary>
    public static string? LightMode => Glyph("\ue518");
    /// <summary>Official icon: <c>light_mode_auto</c></summary>
    public static string? LightModeAuto => Glyph("\U000FFF00");
    /// <summary>Official icon: <c>light_off</c></summary>
    public static string? LightOff => Glyph("\ue9b8");
    /// <summary>Official icon: <c>lightbulb</c></summary>
    public static string? Lightbulb => Glyph("\ue90f");
    /// <summary>Official icon: <c>lightbulb_2</c></summary>
    public static string? Lightbulb2 => Glyph("\uf3e3");
    /// <summary>Official icon: <c>lightbulb_circle</c></summary>
    public static string? LightbulbCircle => Glyph("\uebfe");
    /// <summary>Official icon: <c>lightbulb_outline</c></summary>
    public static string? LightbulbOutline => Glyph("\ue90f");
    /// <summary>Official icon: <c>lightning_stand</c></summary>
    public static string? LightningStand => Glyph("\uefa4");
    /// <summary>Official icon: <c>lightstrip</c></summary>
    public static string? Lightstrip => Glyph("\U000FFF75");
    /// <summary>Official icon: <c>line_axis</c></summary>
    public static string? LineAxis => Glyph("\uea9a");
    /// <summary>Official icon: <c>line_curve</c></summary>
    public static string? LineCurve => Glyph("\uf757");
    /// <summary>Official icon: <c>line_end</c></summary>
    public static string? LineEnd => Glyph("\uf826");
    /// <summary>Official icon: <c>line_end_arrow</c></summary>
    public static string? LineEndArrow => Glyph("\uf81d");
    /// <summary>Official icon: <c>line_end_arrow_notch</c></summary>
    public static string? LineEndArrowNotch => Glyph("\uf81c");
    /// <summary>Official icon: <c>line_end_circle</c></summary>
    public static string? LineEndCircle => Glyph("\uf81b");
    /// <summary>Official icon: <c>line_end_diamond</c></summary>
    public static string? LineEndDiamond => Glyph("\uf81a");
    /// <summary>Official icon: <c>line_end_square</c></summary>
    public static string? LineEndSquare => Glyph("\uf819");
    /// <summary>Official icon: <c>line_start</c></summary>
    public static string? LineStart => Glyph("\uf825");
    /// <summary>Official icon: <c>line_start_arrow</c></summary>
    public static string? LineStartArrow => Glyph("\uf818");
    /// <summary>Official icon: <c>line_start_arrow_notch</c></summary>
    public static string? LineStartArrowNotch => Glyph("\uf817");
    /// <summary>Official icon: <c>line_start_circle</c></summary>
    public static string? LineStartCircle => Glyph("\uf816");
    /// <summary>Official icon: <c>line_start_diamond</c></summary>
    public static string? LineStartDiamond => Glyph("\uf815");
    /// <summary>Official icon: <c>line_start_square</c></summary>
    public static string? LineStartSquare => Glyph("\uf814");
    /// <summary>Official icon: <c>line_style</c></summary>
    public static string? LineStyle => Glyph("\ue919");
    /// <summary>Official icon: <c>line_weight</c></summary>
    public static string? LineWeight => Glyph("\ue91a");
    /// <summary>Official icon: <c>linear_scale</c></summary>
    public static string? LinearScale => Glyph("\ue260");
    /// <summary>Official icon: <c>link</c></summary>
    public static string? Link => Glyph("\ue250");
    /// <summary>Official icon: <c>link_2</c></summary>
    public static string? Link2 => Glyph("\U000FFFB5");
    /// <summary>Official icon: <c>link_off</c></summary>
    public static string? LinkOff => Glyph("\ue16f");
    /// <summary>Official icon: <c>linked_camera</c></summary>
    public static string? LinkedCamera => Glyph("\ue438");
    /// <summary>Official icon: <c>linked_services</c></summary>
    public static string? LinkedServices => Glyph("\uf535");
    /// <summary>Official icon: <c>lips</c></summary>
    public static string? Lips => Glyph("\ueeb2");
    /// <summary>Official icon: <c>liquor</c></summary>
    public static string? Liquor => Glyph("\uea60");
    /// <summary>Official icon: <c>list</c></summary>
    public static string? List => Glyph("\ue896");
    /// <summary>Official icon: <c>list_2</c></summary>
    public static string? List2 => Glyph("\U000FFECA");
    /// <summary>Official icon: <c>list_alt</c></summary>
    public static string? ListAlt => Glyph("\ue0ee");
    /// <summary>Official icon: <c>list_alt_add</c></summary>
    public static string? ListAltAdd => Glyph("\uf756");
    /// <summary>Official icon: <c>list_alt_check</c></summary>
    public static string? ListAltCheck => Glyph("\uf3de");
    /// <summary>Official icon: <c>list_arrow</c></summary>
    public static string? ListArrow => Glyph("\U000FFF33");
    /// <summary>Official icon: <c>lists</c></summary>
    public static string? Lists => Glyph("\ue9b9");
    /// <summary>Official icon: <c>live_help</c></summary>
    public static string? LiveHelp => Glyph("\ue0c6");
    /// <summary>Official icon: <c>live_tv</c></summary>
    public static string? LiveTv => Glyph("\ue63a");
    /// <summary>Official icon: <c>living</c></summary>
    public static string? Living => Glyph("\uf02b");
    /// <summary>Official icon: <c>local_activity</c></summary>
    public static string? LocalActivity => Glyph("\ue553");
    /// <summary>Official icon: <c>local_airport</c></summary>
    public static string? LocalAirport => Glyph("\ue53d");
    /// <summary>Official icon: <c>local_atm</c></summary>
    public static string? LocalAtm => Glyph("\ue53e");
    /// <summary>Official icon: <c>local_bar</c></summary>
    public static string? LocalBar => Glyph("\ue540");
    /// <summary>Official icon: <c>local_cafe</c></summary>
    public static string? LocalCafe => Glyph("\ueb44");
    /// <summary>Official icon: <c>local_car_wash</c></summary>
    public static string? LocalCarWash => Glyph("\ue542");
    /// <summary>Official icon: <c>local_convenience_store</c></summary>
    public static string? LocalConvenienceStore => Glyph("\ue543");
    /// <summary>Official icon: <c>local_dining</c></summary>
    public static string? LocalDining => Glyph("\ue561");
    /// <summary>Official icon: <c>local_drink</c></summary>
    public static string? LocalDrink => Glyph("\ue544");
    /// <summary>Official icon: <c>local_fire_department</c></summary>
    public static string? LocalFireDepartment => Glyph("\uef55");
    /// <summary>Official icon: <c>local_florist</c></summary>
    public static string? LocalFlorist => Glyph("\ue545");
    /// <summary>Official icon: <c>local_gas_station</c></summary>
    public static string? LocalGasStation => Glyph("\ue546");
    /// <summary>Official icon: <c>local_grocery_store</c></summary>
    public static string? LocalGroceryStore => Glyph("\ue8cc");
    /// <summary>Official icon: <c>local_hospital</c></summary>
    public static string? LocalHospital => Glyph("\ue548");
    /// <summary>Official icon: <c>local_hotel</c></summary>
    public static string? LocalHotel => Glyph("\ue549");
    /// <summary>Official icon: <c>local_laundry_service</c></summary>
    public static string? LocalLaundryService => Glyph("\ue54a");
    /// <summary>Official icon: <c>local_library</c></summary>
    public static string? LocalLibrary => Glyph("\ue54b");
    /// <summary>Official icon: <c>local_mall</c></summary>
    public static string? LocalMall => Glyph("\ue54c");
    /// <summary>Official icon: <c>local_movies</c></summary>
    public static string? LocalMovies => Glyph("\ue8da");
    /// <summary>Official icon: <c>local_offer</c></summary>
    public static string? LocalOffer => Glyph("\uf05b");
    /// <summary>Official icon: <c>local_parking</c></summary>
    public static string? LocalParking => Glyph("\ue54f");
    /// <summary>Official icon: <c>local_pharmacy</c></summary>
    public static string? LocalPharmacy => Glyph("\ue550");
    /// <summary>Official icon: <c>local_phone</c></summary>
    public static string? LocalPhone => Glyph("\uf0d4");
    /// <summary>Official icon: <c>local_pizza</c></summary>
    public static string? LocalPizza => Glyph("\ue552");
    /// <summary>Official icon: <c>local_play</c></summary>
    public static string? LocalPlay => Glyph("\ue553");
    /// <summary>Official icon: <c>local_police</c></summary>
    public static string? LocalPolice => Glyph("\uef56");
    /// <summary>Official icon: <c>local_post_office</c></summary>
    public static string? LocalPostOffice => Glyph("\ue554");
    /// <summary>Official icon: <c>local_printshop</c></summary>
    public static string? LocalPrintshop => Glyph("\ue8ad");
    /// <summary>Official icon: <c>local_see</c></summary>
    public static string? LocalSee => Glyph("\ue557");
    /// <summary>Official icon: <c>local_shipping</c></summary>
    public static string? LocalShipping => Glyph("\ue558");
    /// <summary>Official icon: <c>local_taxi</c></summary>
    public static string? LocalTaxi => Glyph("\ue559");
    /// <summary>Official icon: <c>location_automation</c></summary>
    public static string? LocationAutomation => Glyph("\uf14f");
    /// <summary>Official icon: <c>location_away</c></summary>
    public static string? LocationAway => Glyph("\uf150");
    /// <summary>Official icon: <c>location_chip</c></summary>
    public static string? LocationChip => Glyph("\uf850");
    /// <summary>Official icon: <c>location_city</c></summary>
    public static string? LocationCity => Glyph("\ue7f1");
    /// <summary>Official icon: <c>location_disabled</c></summary>
    public static string? LocationDisabled => Glyph("\ue1b6");
    /// <summary>Official icon: <c>location_home</c></summary>
    public static string? LocationHome => Glyph("\uf152");
    /// <summary>Official icon: <c>location_off</c></summary>
    public static string? LocationOff => Glyph("\ue0c7");
    /// <summary>Official icon: <c>location_on</c></summary>
    public static string? LocationOn => Glyph("\uf1db");
    /// <summary>Official icon: <c>location_pin</c></summary>
    public static string? LocationPin => Glyph("\uf1db");
    /// <summary>Official icon: <c>location_searching</c></summary>
    public static string? LocationSearching => Glyph("\ue1b7");
    /// <summary>Official icon: <c>locator_tag</c></summary>
    public static string? LocatorTag => Glyph("\uf8c1");
    /// <summary>Official icon: <c>lock</c></summary>
    public static string? Lock => Glyph("\ue899");
    /// <summary>Official icon: <c>lock_clock</c></summary>
    public static string? LockClock => Glyph("\uef57");
    /// <summary>Official icon: <c>lock_open</c></summary>
    public static string? LockOpen => Glyph("\ue898");
    /// <summary>Official icon: <c>lock_open_circle</c></summary>
    public static string? LockOpenCircle => Glyph("\uf361");
    /// <summary>Official icon: <c>lock_open_right</c></summary>
    public static string? LockOpenRight => Glyph("\uf656");
    /// <summary>Official icon: <c>lock_outline</c></summary>
    public static string? LockOutline => Glyph("\ue899");
    /// <summary>Official icon: <c>lock_person</c></summary>
    public static string? LockPerson => Glyph("\uf8f3");
    /// <summary>Official icon: <c>lock_reset</c></summary>
    public static string? LockReset => Glyph("\ueade");
    /// <summary>Official icon: <c>login</c></summary>
    public static string? Login => Glyph("\uea77");
    /// <summary>Official icon: <c>logo_dev</c></summary>
    public static string? LogoDev => Glyph("\uead6");
    /// <summary>Official icon: <c>logout</c></summary>
    public static string? Logout => Glyph("\ue9ba");
    /// <summary>Official icon: <c>looks</c></summary>
    public static string? Looks => Glyph("\ue3fc");
    /// <summary>Official icon: <c>looks_3</c></summary>
    public static string? Looks3 => Glyph("\ue3fb");
    /// <summary>Official icon: <c>looks_4</c></summary>
    public static string? Looks4 => Glyph("\ue3fd");
    /// <summary>Official icon: <c>looks_5</c></summary>
    public static string? Looks5 => Glyph("\ue3fe");
    /// <summary>Official icon: <c>looks_6</c></summary>
    public static string? Looks6 => Glyph("\ue3ff");
    /// <summary>Official icon: <c>looks_one</c></summary>
    public static string? LooksOne => Glyph("\ue400");
    /// <summary>Official icon: <c>looks_two</c></summary>
    public static string? LooksTwo => Glyph("\ue401");
    /// <summary>Official icon: <c>loop</c></summary>
    public static string? Loop => Glyph("\ue863");
    /// <summary>Official icon: <c>loupe</c></summary>
    public static string? Loupe => Glyph("\ue402");
    /// <summary>Official icon: <c>low_density</c></summary>
    public static string? LowDensity => Glyph("\uf79b");
    /// <summary>Official icon: <c>low_priority</c></summary>
    public static string? LowPriority => Glyph("\ue16d");
    /// <summary>Official icon: <c>lowercase</c></summary>
    public static string? Lowercase => Glyph("\uf48a");
    /// <summary>Official icon: <c>loyalty</c></summary>
    public static string? Loyalty => Glyph("\ue89a");
    /// <summary>Official icon: <c>lte_mobiledata</c></summary>
    public static string? LteMobiledata => Glyph("\uf02c");
    /// <summary>Official icon: <c>lte_mobiledata_badge</c></summary>
    public static string? LteMobiledataBadge => Glyph("\uf7d9");
    /// <summary>Official icon: <c>lte_plus_mobiledata</c></summary>
    public static string? LtePlusMobiledata => Glyph("\uf02d");
    /// <summary>Official icon: <c>lte_plus_mobiledata_badge</c></summary>
    public static string? LtePlusMobiledataBadge => Glyph("\uf7d8");
    /// <summary>Official icon: <c>luggage</c></summary>
    public static string? Luggage => Glyph("\uf235");
    /// <summary>Official icon: <c>lunch_dining</c></summary>
    public static string? LunchDining => Glyph("\uea61");
    /// <summary>Official icon: <c>lyrics</c></summary>
    public static string? Lyrics => Glyph("\uec0b");
    /// <summary>Official icon: <c>macro_auto</c></summary>
    public static string? MacroAuto => Glyph("\uf6f2");
    /// <summary>Official icon: <c>macro_off</c></summary>
    public static string? MacroOff => Glyph("\uf8d2");
    /// <summary>Official icon: <c>magic_button</c></summary>
    public static string? MagicButton => Glyph("\uf136");
    /// <summary>Official icon: <c>magic_exchange</c></summary>
    public static string? MagicExchange => Glyph("\uf7f4");
    /// <summary>Official icon: <c>magic_tether</c></summary>
    public static string? MagicTether => Glyph("\uf7d7");
    /// <summary>Official icon: <c>magnification_large</c></summary>
    public static string? MagnificationLarge => Glyph("\uf83d");
    /// <summary>Official icon: <c>magnification_small</c></summary>
    public static string? MagnificationSmall => Glyph("\uf83c");
    /// <summary>Official icon: <c>magnify_docked</c></summary>
    public static string? MagnifyDocked => Glyph("\uf7d6");
    /// <summary>Official icon: <c>magnify_fullscreen</c></summary>
    public static string? MagnifyFullscreen => Glyph("\uf7d5");
    /// <summary>Official icon: <c>mail</c></summary>
    public static string? Mail => Glyph("\ue159");
    /// <summary>Official icon: <c>mail_asterisk</c></summary>
    public static string? MailAsterisk => Glyph("\ueef4");
    /// <summary>Official icon: <c>mail_lock</c></summary>
    public static string? MailLock => Glyph("\uec0a");
    /// <summary>Official icon: <c>mail_off</c></summary>
    public static string? MailOff => Glyph("\uf48b");
    /// <summary>Official icon: <c>mail_outline</c></summary>
    public static string? MailOutline => Glyph("\ue159");
    /// <summary>Official icon: <c>mail_shield</c></summary>
    public static string? MailShield => Glyph("\uf249");
    /// <summary>Official icon: <c>male</c></summary>
    public static string? Male => Glyph("\ue58e");
    /// <summary>Official icon: <c>man</c></summary>
    public static string? Man => Glyph("\ue4eb");
    /// <summary>Official icon: <c>man_2</c></summary>
    public static string? Man2 => Glyph("\uf8e1");
    /// <summary>Official icon: <c>man_3</c></summary>
    public static string? Man3 => Glyph("\uf8e2");
    /// <summary>Official icon: <c>man_4</c></summary>
    public static string? Man4 => Glyph("\uf8e3");
    /// <summary>Official icon: <c>manage_accounts</c></summary>
    public static string? ManageAccounts => Glyph("\uf02e");
    /// <summary>Official icon: <c>manage_history</c></summary>
    public static string? ManageHistory => Glyph("\uebe7");
    /// <summary>Official icon: <c>manage_search</c></summary>
    public static string? ManageSearch => Glyph("\uf02f");
    /// <summary>Official icon: <c>manga</c></summary>
    public static string? Manga => Glyph("\uf5e3");
    /// <summary>Official icon: <c>manufacturing</c></summary>
    public static string? Manufacturing => Glyph("\ue726");
    /// <summary>Official icon: <c>map</c></summary>
    public static string? Map => Glyph("\ue55b");
    /// <summary>Official icon: <c>map_pin_heart</c></summary>
    public static string? MapPinHeart => Glyph("\uf298");
    /// <summary>Official icon: <c>map_pin_review</c></summary>
    public static string? MapPinReview => Glyph("\uf297");
    /// <summary>Official icon: <c>map_search</c></summary>
    public static string? MapSearch => Glyph("\uf3ca");
    /// <summary>Official icon: <c>maps_home_work</c></summary>
    public static string? MapsHomeWork => Glyph("\uf030");
    /// <summary>Official icon: <c>maps_ugc</c></summary>
    public static string? MapsUgc => Glyph("\uef58");
    /// <summary>Official icon: <c>margin</c></summary>
    public static string? Margin => Glyph("\ue9bb");
    /// <summary>Official icon: <c>mark_as_unread</c></summary>
    public static string? MarkAsUnread => Glyph("\ue9bc");
    /// <summary>Official icon: <c>mark_chat_read</c></summary>
    public static string? MarkChatRead => Glyph("\uf18b");
    /// <summary>Official icon: <c>mark_chat_unread</c></summary>
    public static string? MarkChatUnread => Glyph("\uf189");
    /// <summary>Official icon: <c>mark_email_read</c></summary>
    public static string? MarkEmailRead => Glyph("\uf18c");
    /// <summary>Official icon: <c>mark_email_unread</c></summary>
    public static string? MarkEmailUnread => Glyph("\uf18a");
    /// <summary>Official icon: <c>mark_unread_chat_alt</c></summary>
    public static string? MarkUnreadChatAlt => Glyph("\ueb9d");
    /// <summary>Official icon: <c>markdown</c></summary>
    public static string? Markdown => Glyph("\uf552");
    /// <summary>Official icon: <c>markdown_copy</c></summary>
    public static string? MarkdownCopy => Glyph("\uf553");
    /// <summary>Official icon: <c>markdown_paste</c></summary>
    public static string? MarkdownPaste => Glyph("\uf554");
    /// <summary>Official icon: <c>markunread</c></summary>
    public static string? Markunread => Glyph("\ue159");
    /// <summary>Official icon: <c>markunread_mailbox</c></summary>
    public static string? MarkunreadMailbox => Glyph("\ue89b");
    /// <summary>Official icon: <c>masked_transitions</c></summary>
    public static string? MaskedTransitions => Glyph("\ue72e");
    /// <summary>Official icon: <c>masked_transitions_add</c></summary>
    public static string? MaskedTransitionsAdd => Glyph("\uf42b");
    /// <summary>Official icon: <c>masks</c></summary>
    public static string? Masks => Glyph("\uf218");
    /// <summary>Official icon: <c>massage</c></summary>
    public static string? Massage => Glyph("\uf2c2");
    /// <summary>Official icon: <c>match_case</c></summary>
    public static string? MatchCase => Glyph("\uf6f1");
    /// <summary>Official icon: <c>match_case_off</c></summary>
    public static string? MatchCaseOff => Glyph("\uf36f");
    /// <summary>Official icon: <c>match_word</c></summary>
    public static string? MatchWord => Glyph("\uf6f0");
    /// <summary>Official icon: <c>matter</c></summary>
    public static string? Matter => Glyph("\ue907");
    /// <summary>Official icon: <c>maximize</c></summary>
    public static string? Maximize => Glyph("\ue930");
    /// <summary>Official icon: <c>meal_dinner</c></summary>
    public static string? MealDinner => Glyph("\uf23d");
    /// <summary>Official icon: <c>meal_lunch</c></summary>
    public static string? MealLunch => Glyph("\uf23c");
    /// <summary>Official icon: <c>measuring_tape</c></summary>
    public static string? MeasuringTape => Glyph("\uf6af");
    /// <summary>Official icon: <c>media_bluetooth_off</c></summary>
    public static string? MediaBluetoothOff => Glyph("\uf031");
    /// <summary>Official icon: <c>media_bluetooth_on</c></summary>
    public static string? MediaBluetoothOn => Glyph("\uf032");
    /// <summary>Official icon: <c>media_link</c></summary>
    public static string? MediaLink => Glyph("\uf83f");
    /// <summary>Official icon: <c>media_output</c></summary>
    public static string? MediaOutput => Glyph("\uf4f2");
    /// <summary>Official icon: <c>media_output_off</c></summary>
    public static string? MediaOutputOff => Glyph("\uf4f3");
    /// <summary>Official icon: <c>mediation</c></summary>
    public static string? Mediation => Glyph("\uefa7");
    /// <summary>Official icon: <c>medical_information</c></summary>
    public static string? MedicalInformation => Glyph("\uebed");
    /// <summary>Official icon: <c>medical_mask</c></summary>
    public static string? MedicalMask => Glyph("\uf80a");
    /// <summary>Official icon: <c>medical_services</c></summary>
    public static string? MedicalServices => Glyph("\uf109");
    /// <summary>Official icon: <c>medication</c></summary>
    public static string? Medication => Glyph("\uf033");
    /// <summary>Official icon: <c>medication_liquid</c></summary>
    public static string? MedicationLiquid => Glyph("\uea87");
    /// <summary>Official icon: <c>meeting_room</c></summary>
    public static string? MeetingRoom => Glyph("\ueb4f");
    /// <summary>Official icon: <c>memory</c></summary>
    public static string? Memory => Glyph("\ue322");
    /// <summary>Official icon: <c>memory_alt</c></summary>
    public static string? MemoryAlt => Glyph("\uf7a3");
    /// <summary>Official icon: <c>menstrual_health</c></summary>
    public static string? MenstrualHealth => Glyph("\uf6e1");
    /// <summary>Official icon: <c>menu</c></summary>
    public static string? Menu => Glyph("\ue5d2");
    /// <summary>Official icon: <c>menu_book</c></summary>
    public static string? MenuBook => Glyph("\uea19");
    /// <summary>Official icon: <c>menu_book_2</c></summary>
    public static string? MenuBook2 => Glyph("\uf291");
    /// <summary>Official icon: <c>menu_open</c></summary>
    public static string? MenuOpen => Glyph("\ue9bd");
    /// <summary>Official icon: <c>merge</c></summary>
    public static string? Merge => Glyph("\ueb98");
    /// <summary>Official icon: <c>merge_type</c></summary>
    public static string? MergeType => Glyph("\ue252");
    /// <summary>Official icon: <c>message</c></summary>
    public static string? Message => Glyph("\ue0c9");
    /// <summary>Official icon: <c>metabolism</c></summary>
    public static string? Metabolism => Glyph("\ue10b");
    /// <summary>Official icon: <c>metro</c></summary>
    public static string? Metro => Glyph("\uf474");
    /// <summary>Official icon: <c>mfg_nest_yale_lock</c></summary>
    public static string? MfgNestYaleLock => Glyph("\uf11d");
    /// <summary>Official icon: <c>mic</c></summary>
    public static string? Mic => Glyph("\ue31d");
    /// <summary>Official icon: <c>mic_alert</c></summary>
    public static string? MicAlert => Glyph("\uf392");
    /// <summary>Official icon: <c>mic_double</c></summary>
    public static string? MicDouble => Glyph("\uf5d1");
    /// <summary>Official icon: <c>mic_external_off</c></summary>
    public static string? MicExternalOff => Glyph("\uef59");
    /// <summary>Official icon: <c>mic_external_on</c></summary>
    public static string? MicExternalOn => Glyph("\uef5a");
    /// <summary>Official icon: <c>mic_gear</c></summary>
    public static string? MicGear => Glyph("\ueeba");
    /// <summary>Official icon: <c>mic_none</c></summary>
    public static string? MicNone => Glyph("\ue31d");
    /// <summary>Official icon: <c>mic_off</c></summary>
    public static string? MicOff => Glyph("\ue02b");
    /// <summary>Official icon: <c>microbiology</c></summary>
    public static string? Microbiology => Glyph("\ue10c");
    /// <summary>Official icon: <c>microwave</c></summary>
    public static string? Microwave => Glyph("\uf204");
    /// <summary>Official icon: <c>microwave_gen</c></summary>
    public static string? MicrowaveGen => Glyph("\ue847");
    /// <summary>Official icon: <c>military_tech</c></summary>
    public static string? MilitaryTech => Glyph("\uea3f");
    /// <summary>Official icon: <c>mimo</c></summary>
    public static string? Mimo => Glyph("\ue9be");
    /// <summary>Official icon: <c>mimo_disconnect</c></summary>
    public static string? MimoDisconnect => Glyph("\ue9bf");
    /// <summary>Official icon: <c>mindfulness</c></summary>
    public static string? Mindfulness => Glyph("\uf6e0");
    /// <summary>Official icon: <c>minimize</c></summary>
    public static string? Minimize => Glyph("\ue931");
    /// <summary>Official icon: <c>minor_crash</c></summary>
    public static string? MinorCrash => Glyph("\uebf1");
    /// <summary>Official icon: <c>mintmark</c></summary>
    public static string? Mintmark => Glyph("\uefa9");
    /// <summary>Official icon: <c>missed_video_call</c></summary>
    public static string? MissedVideoCall => Glyph("\uf0ce");
    /// <summary>Official icon: <c>missed_video_call_filled</c></summary>
    public static string? MissedVideoCallFilled => Glyph("\uf0ce");
    /// <summary>Official icon: <c>missing_controller</c></summary>
    public static string? MissingController => Glyph("\ue701");
    /// <summary>Official icon: <c>mist</c></summary>
    public static string? Mist => Glyph("\ue188");
    /// <summary>Official icon: <c>mitre</c></summary>
    public static string? Mitre => Glyph("\uf547");
    /// <summary>Official icon: <c>mixture_med</c></summary>
    public static string? MixtureMed => Glyph("\ue4c8");
    /// <summary>Official icon: <c>mms</c></summary>
    public static string? Mms => Glyph("\ue618");
    /// <summary>Official icon: <c>mobile</c></summary>
    public static string? Mobile => Glyph("\ue7ba");
    /// <summary>Official icon: <c>mobile_2</c></summary>
    public static string? Mobile2 => Glyph("\uf2db");
    /// <summary>Official icon: <c>mobile_3</c></summary>
    public static string? Mobile3 => Glyph("\uf2da");
    /// <summary>Official icon: <c>mobile_alert</c></summary>
    public static string? MobileAlert => Glyph("\uf2d3");
    /// <summary>Official icon: <c>mobile_arrow_down</c></summary>
    public static string? MobileArrowDown => Glyph("\uf2cd");
    /// <summary>Official icon: <c>mobile_arrow_right</c></summary>
    public static string? MobileArrowRight => Glyph("\uf2d2");
    /// <summary>Official icon: <c>mobile_arrow_up_right</c></summary>
    public static string? MobileArrowUpRight => Glyph("\uf2b9");
    /// <summary>Official icon: <c>mobile_block</c></summary>
    public static string? MobileBlock => Glyph("\uf2e5");
    /// <summary>Official icon: <c>mobile_camera</c></summary>
    public static string? MobileCamera => Glyph("\uf44e");
    /// <summary>Official icon: <c>mobile_camera_front</c></summary>
    public static string? MobileCameraFront => Glyph("\uf2c9");
    /// <summary>Official icon: <c>mobile_camera_rear</c></summary>
    public static string? MobileCameraRear => Glyph("\uf2c8");
    /// <summary>Official icon: <c>mobile_cancel</c></summary>
    public static string? MobileCancel => Glyph("\uf2ea");
    /// <summary>Official icon: <c>mobile_cast</c></summary>
    public static string? MobileCast => Glyph("\uf2cc");
    /// <summary>Official icon: <c>mobile_charge</c></summary>
    public static string? MobileCharge => Glyph("\uf2e3");
    /// <summary>Official icon: <c>mobile_chat</c></summary>
    public static string? MobileChat => Glyph("\uf79f");
    /// <summary>Official icon: <c>mobile_check</c></summary>
    public static string? MobileCheck => Glyph("\uf073");
    /// <summary>Official icon: <c>mobile_code</c></summary>
    public static string? MobileCode => Glyph("\uf2e2");
    /// <summary>Official icon: <c>mobile_dock</c></summary>
    public static string? MobileDock => Glyph("\uf2e0");
    /// <summary>Official icon: <c>mobile_dots</c></summary>
    public static string? MobileDots => Glyph("\uf2d0");
    /// <summary>Official icon: <c>mobile_friendly</c></summary>
    public static string? MobileFriendly => Glyph("\uf073");
    /// <summary>Official icon: <c>mobile_gear</c></summary>
    public static string? MobileGear => Glyph("\uf2d9");
    /// <summary>Official icon: <c>mobile_hand</c></summary>
    public static string? MobileHand => Glyph("\uf323");
    /// <summary>Official icon: <c>mobile_hand_left</c></summary>
    public static string? MobileHandLeft => Glyph("\uf313");
    /// <summary>Official icon: <c>mobile_hand_left_off</c></summary>
    public static string? MobileHandLeftOff => Glyph("\uf312");
    /// <summary>Official icon: <c>mobile_hand_off</c></summary>
    public static string? MobileHandOff => Glyph("\uf314");
    /// <summary>Official icon: <c>mobile_info</c></summary>
    public static string? MobileInfo => Glyph("\uf2dc");
    /// <summary>Official icon: <c>mobile_landscape</c></summary>
    public static string? MobileLandscape => Glyph("\ued3e");
    /// <summary>Official icon: <c>mobile_layout</c></summary>
    public static string? MobileLayout => Glyph("\uf2bf");
    /// <summary>Official icon: <c>mobile_lock_landscape</c></summary>
    public static string? MobileLockLandscape => Glyph("\uf2d8");
    /// <summary>Official icon: <c>mobile_lock_portrait</c></summary>
    public static string? MobileLockPortrait => Glyph("\uf2be");
    /// <summary>Official icon: <c>mobile_loupe</c></summary>
    public static string? MobileLoupe => Glyph("\uf322");
    /// <summary>Official icon: <c>mobile_menu</c></summary>
    public static string? MobileMenu => Glyph("\uf2d1");
    /// <summary>Official icon: <c>mobile_off</c></summary>
    public static string? MobileOff => Glyph("\ue201");
    /// <summary>Official icon: <c>mobile_question</c></summary>
    public static string? MobileQuestion => Glyph("\uf2e1");
    /// <summary>Official icon: <c>mobile_rotate</c></summary>
    public static string? MobileRotate => Glyph("\uf2d5");
    /// <summary>Official icon: <c>mobile_rotate_lock</c></summary>
    public static string? MobileRotateLock => Glyph("\uf2d6");
    /// <summary>Official icon: <c>mobile_screen_share</c></summary>
    public static string? MobileScreenShare => Glyph("\uf2df");
    /// <summary>Official icon: <c>mobile_screensaver</c></summary>
    public static string? MobileScreensaver => Glyph("\uf321");
    /// <summary>Official icon: <c>mobile_sensor_hi</c></summary>
    public static string? MobileSensorHi => Glyph("\uf2ef");
    /// <summary>Official icon: <c>mobile_sensor_lo</c></summary>
    public static string? MobileSensorLo => Glyph("\uf2ee");
    /// <summary>Official icon: <c>mobile_share</c></summary>
    public static string? MobileShare => Glyph("\uf2df");
    /// <summary>Official icon: <c>mobile_share_stack</c></summary>
    public static string? MobileShareStack => Glyph("\uf2de");
    /// <summary>Official icon: <c>mobile_sound</c></summary>
    public static string? MobileSound => Glyph("\uf2e8");
    /// <summary>Official icon: <c>mobile_sound_2</c></summary>
    public static string? MobileSound2 => Glyph("\uf318");
    /// <summary>Official icon: <c>mobile_sound_off</c></summary>
    public static string? MobileSoundOff => Glyph("\uf7aa");
    /// <summary>Official icon: <c>mobile_speaker</c></summary>
    public static string? MobileSpeaker => Glyph("\uf320");
    /// <summary>Official icon: <c>mobile_tap</c></summary>
    public static string? MobileTap => Glyph("\U000FFEB2");
    /// <summary>Official icon: <c>mobile_text</c></summary>
    public static string? MobileText => Glyph("\uf2eb");
    /// <summary>Official icon: <c>mobile_text_2</c></summary>
    public static string? MobileText2 => Glyph("\uf2e6");
    /// <summary>Official icon: <c>mobile_theft</c></summary>
    public static string? MobileTheft => Glyph("\uf2a9");
    /// <summary>Official icon: <c>mobile_ticket</c></summary>
    public static string? MobileTicket => Glyph("\uf2e4");
    /// <summary>Official icon: <c>mobile_unlock</c></summary>
    public static string? MobileUnlock => Glyph("\ueeea");
    /// <summary>Official icon: <c>mobile_vibrate</c></summary>
    public static string? MobileVibrate => Glyph("\uf2cb");
    /// <summary>Official icon: <c>mobile_wrench</c></summary>
    public static string? MobileWrench => Glyph("\uf2b0");
    /// <summary>Official icon: <c>mobiledata_arrows</c></summary>
    public static string? MobiledataArrows => Glyph("\U000FFFA3");
    /// <summary>Official icon: <c>mobiledata_off</c></summary>
    public static string? MobiledataOff => Glyph("\uf034");
    /// <summary>Official icon: <c>mode</c></summary>
    public static string? Mode => Glyph("\uf097");
    /// <summary>Official icon: <c>mode_comment</c></summary>
    public static string? ModeComment => Glyph("\ue253");
    /// <summary>Official icon: <c>mode_cool</c></summary>
    public static string? ModeCool => Glyph("\uf166");
    /// <summary>Official icon: <c>mode_cool_off</c></summary>
    public static string? ModeCoolOff => Glyph("\uf167");
    /// <summary>Official icon: <c>mode_dual</c></summary>
    public static string? ModeDual => Glyph("\uf557");
    /// <summary>Official icon: <c>mode_edit</c></summary>
    public static string? ModeEdit => Glyph("\uf097");
    /// <summary>Official icon: <c>mode_edit_outline</c></summary>
    public static string? ModeEditOutline => Glyph("\uf097");
    /// <summary>Official icon: <c>mode_fan</c></summary>
    public static string? ModeFan => Glyph("\uf168");
    /// <summary>Official icon: <c>mode_fan_2</c></summary>
    public static string? ModeFan2 => Glyph("\U000FFFD0");
    /// <summary>Official icon: <c>mode_fan_off</c></summary>
    public static string? ModeFanOff => Glyph("\uec17");
    /// <summary>Official icon: <c>mode_heat</c></summary>
    public static string? ModeHeat => Glyph("\uf16a");
    /// <summary>Official icon: <c>mode_heat_cool</c></summary>
    public static string? ModeHeatCool => Glyph("\uf16b");
    /// <summary>Official icon: <c>mode_heat_off</c></summary>
    public static string? ModeHeatOff => Glyph("\uf16d");
    /// <summary>Official icon: <c>mode_night</c></summary>
    public static string? ModeNight => Glyph("\uf036");
    /// <summary>Official icon: <c>mode_of_travel</c></summary>
    public static string? ModeOfTravel => Glyph("\ue7ce");
    /// <summary>Official icon: <c>mode_off_on</c></summary>
    public static string? ModeOffOn => Glyph("\uf16f");
    /// <summary>Official icon: <c>mode_standby</c></summary>
    public static string? ModeStandby => Glyph("\uf037");
    /// <summary>Official icon: <c>model_training</c></summary>
    public static string? ModelTraining => Glyph("\uf0cf");
    /// <summary>Official icon: <c>modeling</c></summary>
    public static string? Modeling => Glyph("\uf3aa");
    /// <summary>Official icon: <c>monetization_on</c></summary>
    public static string? MonetizationOn => Glyph("\ue263");
    /// <summary>Official icon: <c>money</c></summary>
    public static string? Money => Glyph("\ue57d");
    /// <summary>Official icon: <c>money_bag</c></summary>
    public static string? MoneyBag => Glyph("\uf3ee");
    /// <summary>Official icon: <c>money_off</c></summary>
    public static string? MoneyOff => Glyph("\uf038");
    /// <summary>Official icon: <c>money_off_csred</c></summary>
    public static string? MoneyOffCsred => Glyph("\uf038");
    /// <summary>Official icon: <c>money_range</c></summary>
    public static string? MoneyRange => Glyph("\uf245");
    /// <summary>Official icon: <c>monitor</c></summary>
    public static string? Monitor => Glyph("\uef5b");
    /// <summary>Official icon: <c>monitor_heart</c></summary>
    public static string? MonitorHeart => Glyph("\ueaa2");
    /// <summary>Official icon: <c>monitor_weight</c></summary>
    public static string? MonitorWeight => Glyph("\uf039");
    /// <summary>Official icon: <c>monitor_weight_gain</c></summary>
    public static string? MonitorWeightGain => Glyph("\uf6df");
    /// <summary>Official icon: <c>monitor_weight_loss</c></summary>
    public static string? MonitorWeightLoss => Glyph("\uf6de");
    /// <summary>Official icon: <c>monitoring</c></summary>
    public static string? Monitoring => Glyph("\uf190");
    /// <summary>Official icon: <c>monochrome_photos</c></summary>
    public static string? MonochromePhotos => Glyph("\ue403");
    /// <summary>Official icon: <c>monorail</c></summary>
    public static string? Monorail => Glyph("\uf473");
    /// <summary>Official icon: <c>mood</c></summary>
    public static string? Mood => Glyph("\uea22");
    /// <summary>Official icon: <c>mood_bad</c></summary>
    public static string? MoodBad => Glyph("\ue7f3");
    /// <summary>Official icon: <c>mood_heart</c></summary>
    public static string? MoodHeart => Glyph("\U000FFFB4");
    /// <summary>Official icon: <c>moon_stars</c></summary>
    public static string? MoonStars => Glyph("\uf34f");
    /// <summary>Official icon: <c>mop</c></summary>
    public static string? Mop => Glyph("\ue28d");
    /// <summary>Official icon: <c>moped</c></summary>
    public static string? Moped => Glyph("\ueb28");
    /// <summary>Official icon: <c>moped_package</c></summary>
    public static string? MopedPackage => Glyph("\uf28b");
    /// <summary>Official icon: <c>more</c></summary>
    public static string? More => Glyph("\ue619");
    /// <summary>Official icon: <c>more_down</c></summary>
    public static string? MoreDown => Glyph("\uf196");
    /// <summary>Official icon: <c>more_horiz</c></summary>
    public static string? MoreHoriz => Glyph("\ue5d3");
    /// <summary>Official icon: <c>more_time</c></summary>
    public static string? MoreTime => Glyph("\uea5d");
    /// <summary>Official icon: <c>more_up</c></summary>
    public static string? MoreUp => Glyph("\uf197");
    /// <summary>Official icon: <c>more_vert</c></summary>
    public static string? MoreVert => Glyph("\ue5d4");
    /// <summary>Official icon: <c>mosque</c></summary>
    public static string? Mosque => Glyph("\ueab2");
    /// <summary>Official icon: <c>motion_blur</c></summary>
    public static string? MotionBlur => Glyph("\uf0d0");
    /// <summary>Official icon: <c>motion_mode</c></summary>
    public static string? MotionMode => Glyph("\uf842");
    /// <summary>Official icon: <c>motion_photos_auto</c></summary>
    public static string? MotionPhotosAuto => Glyph("\uf03a");
    /// <summary>Official icon: <c>motion_photos_off</c></summary>
    public static string? MotionPhotosOff => Glyph("\ue9c0");
    /// <summary>Official icon: <c>motion_photos_on</c></summary>
    public static string? MotionPhotosOn => Glyph("\ue9c1");
    /// <summary>Official icon: <c>motion_photos_pause</c></summary>
    public static string? MotionPhotosPause => Glyph("\uf227");
    /// <summary>Official icon: <c>motion_photos_paused</c></summary>
    public static string? MotionPhotosPaused => Glyph("\uf227");
    /// <summary>Official icon: <c>motion_play</c></summary>
    public static string? MotionPlay => Glyph("\uf40b");
    /// <summary>Official icon: <c>motion_sensor_active</c></summary>
    public static string? MotionSensorActive => Glyph("\ue792");
    /// <summary>Official icon: <c>motion_sensor_alert</c></summary>
    public static string? MotionSensorAlert => Glyph("\ue784");
    /// <summary>Official icon: <c>motion_sensor_idle</c></summary>
    public static string? MotionSensorIdle => Glyph("\ue783");
    /// <summary>Official icon: <c>motion_sensor_urgent</c></summary>
    public static string? MotionSensorUrgent => Glyph("\ue78e");
    /// <summary>Official icon: <c>motorcycle</c></summary>
    public static string? Motorcycle => Glyph("\ue91b");
    /// <summary>Official icon: <c>mountain_flag</c></summary>
    public static string? MountainFlag => Glyph("\uf5e2");
    /// <summary>Official icon: <c>mountain_steam</c></summary>
    public static string? MountainSteam => Glyph("\uf282");
    /// <summary>Official icon: <c>mouse</c></summary>
    public static string? Mouse => Glyph("\ue323");
    /// <summary>Official icon: <c>mouse_lock</c></summary>
    public static string? MouseLock => Glyph("\uf490");
    /// <summary>Official icon: <c>mouse_lock_off</c></summary>
    public static string? MouseLockOff => Glyph("\uf48f");
    /// <summary>Official icon: <c>move</c></summary>
    public static string? Move => Glyph("\ue740");
    /// <summary>Official icon: <c>move_down</c></summary>
    public static string? MoveDown => Glyph("\ueb61");
    /// <summary>Official icon: <c>move_group</c></summary>
    public static string? MoveGroup => Glyph("\uf715");
    /// <summary>Official icon: <c>move_item</c></summary>
    public static string? MoveItem => Glyph("\uf1ff");
    /// <summary>Official icon: <c>move_location</c></summary>
    public static string? MoveLocation => Glyph("\ue741");
    /// <summary>Official icon: <c>move_selection_down</c></summary>
    public static string? MoveSelectionDown => Glyph("\uf714");
    /// <summary>Official icon: <c>move_selection_left</c></summary>
    public static string? MoveSelectionLeft => Glyph("\uf713");
    /// <summary>Official icon: <c>move_selection_right</c></summary>
    public static string? MoveSelectionRight => Glyph("\uf712");
    /// <summary>Official icon: <c>move_selection_up</c></summary>
    public static string? MoveSelectionUp => Glyph("\uf711");
    /// <summary>Official icon: <c>move_to_inbox</c></summary>
    public static string? MoveToInbox => Glyph("\ue168");
    /// <summary>Official icon: <c>move_up</c></summary>
    public static string? MoveUp => Glyph("\ueb64");
    /// <summary>Official icon: <c>moved_location</c></summary>
    public static string? MovedLocation => Glyph("\ue594");
    /// <summary>Official icon: <c>movie</c></summary>
    public static string? Movie => Glyph("\ue404");
    /// <summary>Official icon: <c>movie_creation</c></summary>
    public static string? MovieCreation => Glyph("\ue404");
    /// <summary>Official icon: <c>movie_edit</c></summary>
    public static string? MovieEdit => Glyph("\uf840");
    /// <summary>Official icon: <c>movie_edit_off</c></summary>
    public static string? MovieEditOff => Glyph("\U000FFF7D");
    /// <summary>Official icon: <c>movie_filter</c></summary>
    public static string? MovieFilter => Glyph("\ue43a");
    /// <summary>Official icon: <c>movie_info</c></summary>
    public static string? MovieInfo => Glyph("\ue02d");
    /// <summary>Official icon: <c>movie_off</c></summary>
    public static string? MovieOff => Glyph("\uf499");
    /// <summary>Official icon: <c>movie_speaker</c></summary>
    public static string? MovieSpeaker => Glyph("\uf2a3");
    /// <summary>Official icon: <c>moving</c></summary>
    public static string? Moving => Glyph("\ue501");
    /// <summary>Official icon: <c>moving_beds</c></summary>
    public static string? MovingBeds => Glyph("\ue73d");
    /// <summary>Official icon: <c>moving_ministry</c></summary>
    public static string? MovingMinistry => Glyph("\ue73e");
    /// <summary>Official icon: <c>mp</c></summary>
    public static string? Mp => Glyph("\ue9c3");
    /// <summary>Official icon: <c>multicooker</c></summary>
    public static string? Multicooker => Glyph("\ue293");
    /// <summary>Official icon: <c>multiline_chart</c></summary>
    public static string? MultilineChart => Glyph("\ue6df");
    /// <summary>Official icon: <c>multimodal_hand_eye</c></summary>
    public static string? MultimodalHandEye => Glyph("\uf41b");
    /// <summary>Official icon: <c>multiple_airports</c></summary>
    public static string? MultipleAirports => Glyph("\uefab");
    /// <summary>Official icon: <c>multiple_stop</c></summary>
    public static string? MultipleStop => Glyph("\uf1b9");
    /// <summary>Official icon: <c>museum</c></summary>
    public static string? Museum => Glyph("\uea36");
    /// <summary>Official icon: <c>music_cast</c></summary>
    public static string? MusicCast => Glyph("\ueb1a");
    /// <summary>Official icon: <c>music_history</c></summary>
    public static string? MusicHistory => Glyph("\uf2c1");
    /// <summary>Official icon: <c>music_note</c></summary>
    public static string? MusicNote => Glyph("\ue405");
    /// <summary>Official icon: <c>music_note_2</c></summary>
    public static string? MusicNote2 => Glyph("\U000FFFD8");
    /// <summary>Official icon: <c>music_note_add</c></summary>
    public static string? MusicNoteAdd => Glyph("\uf391");
    /// <summary>Official icon: <c>music_off</c></summary>
    public static string? MusicOff => Glyph("\ue440");
    /// <summary>Official icon: <c>music_video</c></summary>
    public static string? MusicVideo => Glyph("\ue063");
    /// <summary>Official icon: <c>my_location</c></summary>
    public static string? MyLocation => Glyph("\ue55c");
    /// <summary>Official icon: <c>mystery</c></summary>
    public static string? Mystery => Glyph("\uf5e1");
    /// <summary>Official icon: <c>nat</c></summary>
    public static string? Nat => Glyph("\uef5c");
    /// <summary>Official icon: <c>nature</c></summary>
    public static string? Nature => Glyph("\ue406");
    /// <summary>Official icon: <c>nature_people</c></summary>
    public static string? NaturePeople => Glyph("\ue407");
    /// <summary>Official icon: <c>navigate_before</c></summary>
    public static string? NavigateBefore => Glyph("\ue5cb");
    /// <summary>Official icon: <c>navigate_next</c></summary>
    public static string? NavigateNext => Glyph("\ue5cc");
    /// <summary>Official icon: <c>navigation</c></summary>
    public static string? Navigation => Glyph("\ue55d");
    /// <summary>Official icon: <c>near_me</c></summary>
    public static string? NearMe => Glyph("\ue569");
    /// <summary>Official icon: <c>near_me_disabled</c></summary>
    public static string? NearMeDisabled => Glyph("\uf1ef");
    /// <summary>Official icon: <c>nearby</c></summary>
    public static string? Nearby => Glyph("\ue6b7");
    /// <summary>Official icon: <c>nearby_error</c></summary>
    public static string? NearbyError => Glyph("\uf03b");
    /// <summary>Official icon: <c>nearby_off</c></summary>
    public static string? NearbyOff => Glyph("\uf03c");
    /// <summary>Official icon: <c>nephrology</c></summary>
    public static string? Nephrology => Glyph("\ue10d");
    /// <summary>Official icon: <c>nest_audio</c></summary>
    public static string? NestAudio => Glyph("\uebbf");
    /// <summary>Official icon: <c>nest_cam_floodlight</c></summary>
    public static string? NestCamFloodlight => Glyph("\uf8b7");
    /// <summary>Official icon: <c>nest_cam_indoor</c></summary>
    public static string? NestCamIndoor => Glyph("\uf11e");
    /// <summary>Official icon: <c>nest_cam_iq</c></summary>
    public static string? NestCamIq => Glyph("\uf11f");
    /// <summary>Official icon: <c>nest_cam_iq_outdoor</c></summary>
    public static string? NestCamIqOutdoor => Glyph("\uf120");
    /// <summary>Official icon: <c>nest_cam_magnet_mount</c></summary>
    public static string? NestCamMagnetMount => Glyph("\uf8b8");
    /// <summary>Official icon: <c>nest_cam_outdoor</c></summary>
    public static string? NestCamOutdoor => Glyph("\uf121");
    /// <summary>Official icon: <c>nest_cam_stand</c></summary>
    public static string? NestCamStand => Glyph("\uf8b9");
    /// <summary>Official icon: <c>nest_cam_wall_mount</c></summary>
    public static string? NestCamWallMount => Glyph("\uf8ba");
    /// <summary>Official icon: <c>nest_cam_wired_stand</c></summary>
    public static string? NestCamWiredStand => Glyph("\uec16");
    /// <summary>Official icon: <c>nest_clock_farsight_analog</c></summary>
    public static string? NestClockFarsightAnalog => Glyph("\uf8bb");
    /// <summary>Official icon: <c>nest_clock_farsight_digital</c></summary>
    public static string? NestClockFarsightDigital => Glyph("\uf8bc");
    /// <summary>Official icon: <c>nest_connect</c></summary>
    public static string? NestConnect => Glyph("\uf122");
    /// <summary>Official icon: <c>nest_detect</c></summary>
    public static string? NestDetect => Glyph("\uf123");
    /// <summary>Official icon: <c>nest_display</c></summary>
    public static string? NestDisplay => Glyph("\uf124");
    /// <summary>Official icon: <c>nest_display_max</c></summary>
    public static string? NestDisplayMax => Glyph("\uf125");
    /// <summary>Official icon: <c>nest_doorbell_visitor</c></summary>
    public static string? NestDoorbellVisitor => Glyph("\uf8bd");
    /// <summary>Official icon: <c>nest_eco_leaf</c></summary>
    public static string? NestEcoLeaf => Glyph("\uf8be");
    /// <summary>Official icon: <c>nest_farsight_cool</c></summary>
    public static string? NestFarsightCool => Glyph("\uf27d");
    /// <summary>Official icon: <c>nest_farsight_dual</c></summary>
    public static string? NestFarsightDual => Glyph("\uf27c");
    /// <summary>Official icon: <c>nest_farsight_eco</c></summary>
    public static string? NestFarsightEco => Glyph("\uf27b");
    /// <summary>Official icon: <c>nest_farsight_heat</c></summary>
    public static string? NestFarsightHeat => Glyph("\uf27a");
    /// <summary>Official icon: <c>nest_farsight_seasonal</c></summary>
    public static string? NestFarsightSeasonal => Glyph("\uf279");
    /// <summary>Official icon: <c>nest_farsight_weather</c></summary>
    public static string? NestFarsightWeather => Glyph("\uf8bf");
    /// <summary>Official icon: <c>nest_found_savings</c></summary>
    public static string? NestFoundSavings => Glyph("\uf8c0");
    /// <summary>Official icon: <c>nest_gale_wifi</c></summary>
    public static string? NestGaleWifi => Glyph("\uf579");
    /// <summary>Official icon: <c>nest_heat_link_e</c></summary>
    public static string? NestHeatLinkE => Glyph("\uf126");
    /// <summary>Official icon: <c>nest_heat_link_gen_3</c></summary>
    public static string? NestHeatLinkGen3 => Glyph("\uf127");
    /// <summary>Official icon: <c>nest_hello_doorbell</c></summary>
    public static string? NestHelloDoorbell => Glyph("\ue82c");
    /// <summary>Official icon: <c>nest_locator_tag</c></summary>
    public static string? NestLocatorTag => Glyph("\uf8c1");
    /// <summary>Official icon: <c>nest_mini</c></summary>
    public static string? NestMini => Glyph("\ue789");
    /// <summary>Official icon: <c>nest_multi_room</c></summary>
    public static string? NestMultiRoom => Glyph("\uf8c2");
    /// <summary>Official icon: <c>nest_protect</c></summary>
    public static string? NestProtect => Glyph("\ue68e");
    /// <summary>Official icon: <c>nest_remote</c></summary>
    public static string? NestRemote => Glyph("\uf5db");
    /// <summary>Official icon: <c>nest_remote_comfort_sensor</c></summary>
    public static string? NestRemoteComfortSensor => Glyph("\uf12a");
    /// <summary>Official icon: <c>nest_secure_alarm</c></summary>
    public static string? NestSecureAlarm => Glyph("\uf12b");
    /// <summary>Official icon: <c>nest_sunblock</c></summary>
    public static string? NestSunblock => Glyph("\uf8c3");
    /// <summary>Official icon: <c>nest_tag</c></summary>
    public static string? NestTag => Glyph("\uf8c1");
    /// <summary>Official icon: <c>nest_thermostat</c></summary>
    public static string? NestThermostat => Glyph("\ue68f");
    /// <summary>Official icon: <c>nest_thermostat_e_eu</c></summary>
    public static string? NestThermostatEEu => Glyph("\uf12d");
    /// <summary>Official icon: <c>nest_thermostat_gen_3</c></summary>
    public static string? NestThermostatGen3 => Glyph("\uf12e");
    /// <summary>Official icon: <c>nest_thermostat_sensor</c></summary>
    public static string? NestThermostatSensor => Glyph("\uf12f");
    /// <summary>Official icon: <c>nest_thermostat_sensor_eu</c></summary>
    public static string? NestThermostatSensorEu => Glyph("\uf130");
    /// <summary>Official icon: <c>nest_thermostat_zirconium_eu</c></summary>
    public static string? NestThermostatZirconiumEu => Glyph("\uf131");
    /// <summary>Official icon: <c>nest_true_radiant</c></summary>
    public static string? NestTrueRadiant => Glyph("\uf8c4");
    /// <summary>Official icon: <c>nest_wake_on_approach</c></summary>
    public static string? NestWakeOnApproach => Glyph("\uf8c5");
    /// <summary>Official icon: <c>nest_wake_on_press</c></summary>
    public static string? NestWakeOnPress => Glyph("\uf8c6");
    /// <summary>Official icon: <c>nest_wifi_gale</c></summary>
    public static string? NestWifiGale => Glyph("\uf132");
    /// <summary>Official icon: <c>nest_wifi_mistral</c></summary>
    public static string? NestWifiMistral => Glyph("\uf133");
    /// <summary>Official icon: <c>nest_wifi_point</c></summary>
    public static string? NestWifiPoint => Glyph("\uf134");
    /// <summary>Official icon: <c>nest_wifi_point_vento</c></summary>
    public static string? NestWifiPointVento => Glyph("\uf134");
    /// <summary>Official icon: <c>nest_wifi_pro</c></summary>
    public static string? NestWifiPro => Glyph("\uf56b");
    /// <summary>Official icon: <c>nest_wifi_pro_2</c></summary>
    public static string? NestWifiPro2 => Glyph("\uf56a");
    /// <summary>Official icon: <c>nest_wifi_router</c></summary>
    public static string? NestWifiRouter => Glyph("\uf133");
    /// <summary>Official icon: <c>network_cell</c></summary>
    public static string? NetworkCell => Glyph("\ue1b9");
    /// <summary>Official icon: <c>network_check</c></summary>
    public static string? NetworkCheck => Glyph("\ue640");
    /// <summary>Official icon: <c>network_intel_node</c></summary>
    public static string? NetworkIntelNode => Glyph("\uf371");
    /// <summary>Official icon: <c>network_intelligence</c></summary>
    public static string? NetworkIntelligence => Glyph("\uefac");
    /// <summary>Official icon: <c>network_intelligence_history</c></summary>
    public static string? NetworkIntelligenceHistory => Glyph("\uf5f6");
    /// <summary>Official icon: <c>network_intelligence_update</c></summary>
    public static string? NetworkIntelligenceUpdate => Glyph("\uf5f5");
    /// <summary>Official icon: <c>network_locked</c></summary>
    public static string? NetworkLocked => Glyph("\ue61a");
    /// <summary>Official icon: <c>network_manage</c></summary>
    public static string? NetworkManage => Glyph("\uf7ab");
    /// <summary>Official icon: <c>network_node</c></summary>
    public static string? NetworkNode => Glyph("\uf56e");
    /// <summary>Official icon: <c>network_ping</c></summary>
    public static string? NetworkPing => Glyph("\uebca");
    /// <summary>Official icon: <c>network_wifi</c></summary>
    public static string? NetworkWifi => Glyph("\ue1ba");
    /// <summary>Official icon: <c>network_wifi_1_bar</c></summary>
    public static string? NetworkWifi1Bar => Glyph("\uebe4");
    /// <summary>Official icon: <c>network_wifi_1_bar_locked</c></summary>
    public static string? NetworkWifi1BarLocked => Glyph("\uf58f");
    /// <summary>Official icon: <c>network_wifi_2_bar</c></summary>
    public static string? NetworkWifi2Bar => Glyph("\uebd6");
    /// <summary>Official icon: <c>network_wifi_2_bar_locked</c></summary>
    public static string? NetworkWifi2BarLocked => Glyph("\uf58e");
    /// <summary>Official icon: <c>network_wifi_3_bar</c></summary>
    public static string? NetworkWifi3Bar => Glyph("\uebe1");
    /// <summary>Official icon: <c>network_wifi_3_bar_locked</c></summary>
    public static string? NetworkWifi3BarLocked => Glyph("\uf58d");
    /// <summary>Official icon: <c>network_wifi_locked</c></summary>
    public static string? NetworkWifiLocked => Glyph("\uf532");
    /// <summary>Official icon: <c>neurology</c></summary>
    public static string? Neurology => Glyph("\ue10e");
    /// <summary>Official icon: <c>new_label</c></summary>
    public static string? NewLabel => Glyph("\ue609");
    /// <summary>Official icon: <c>new_releases</c></summary>
    public static string? NewReleases => Glyph("\uef76");
    /// <summary>Official icon: <c>new_window</c></summary>
    public static string? NewWindow => Glyph("\uf710");
    /// <summary>Official icon: <c>news</c></summary>
    public static string? News => Glyph("\ue032");
    /// <summary>Official icon: <c>newsmode</c></summary>
    public static string? Newsmode => Glyph("\uefad");
    /// <summary>Official icon: <c>newspaper</c></summary>
    public static string? Newspaper => Glyph("\ueb81");
    /// <summary>Official icon: <c>newsstand</c></summary>
    public static string? Newsstand => Glyph("\ue9c4");
    /// <summary>Official icon: <c>next_plan</c></summary>
    public static string? NextPlan => Glyph("\uef5d");
    /// <summary>Official icon: <c>next_week</c></summary>
    public static string? NextWeek => Glyph("\ue16a");
    /// <summary>Official icon: <c>nfc</c></summary>
    public static string? Nfc => Glyph("\ue1bb");
    /// <summary>Official icon: <c>nfc_off</c></summary>
    public static string? NfcOff => Glyph("\uf369");
    /// <summary>Official icon: <c>night_shelter</c></summary>
    public static string? NightShelter => Glyph("\uf1f1");
    /// <summary>Official icon: <c>night_sight_auto</c></summary>
    public static string? NightSightAuto => Glyph("\uf1d7");
    /// <summary>Official icon: <c>night_sight_auto_off</c></summary>
    public static string? NightSightAutoOff => Glyph("\uf1f9");
    /// <summary>Official icon: <c>night_sight_max</c></summary>
    public static string? NightSightMax => Glyph("\uf6c3");
    /// <summary>Official icon: <c>nightlife</c></summary>
    public static string? Nightlife => Glyph("\uea62");
    /// <summary>Official icon: <c>nightlight</c></summary>
    public static string? Nightlight => Glyph("\uf03d");
    /// <summary>Official icon: <c>nightlight_round</c></summary>
    public static string? NightlightRound => Glyph("\uf03d");
    /// <summary>Official icon: <c>nights_stay</c></summary>
    public static string? NightsStay => Glyph("\uf174");
    /// <summary>Official icon: <c>no_accounts</c></summary>
    public static string? NoAccounts => Glyph("\uf03e");
    /// <summary>Official icon: <c>no_adult_content</c></summary>
    public static string? NoAdultContent => Glyph("\uf8fe");
    /// <summary>Official icon: <c>no_backpack</c></summary>
    public static string? NoBackpack => Glyph("\uf237");
    /// <summary>Official icon: <c>no_crash</c></summary>
    public static string? NoCrash => Glyph("\uebf0");
    /// <summary>Official icon: <c>no_drinks</c></summary>
    public static string? NoDrinks => Glyph("\uf1a5");
    /// <summary>Official icon: <c>no_encryption</c></summary>
    public static string? NoEncryption => Glyph("\uf03f");
    /// <summary>Official icon: <c>no_encryption_gmailerrorred</c></summary>
    public static string? NoEncryptionGmailerrorred => Glyph("\uf03f");
    /// <summary>Official icon: <c>no_flash</c></summary>
    public static string? NoFlash => Glyph("\uf1a6");
    /// <summary>Official icon: <c>no_food</c></summary>
    public static string? NoFood => Glyph("\uf1a7");
    /// <summary>Official icon: <c>no_luggage</c></summary>
    public static string? NoLuggage => Glyph("\uf23b");
    /// <summary>Official icon: <c>no_meals</c></summary>
    public static string? NoMeals => Glyph("\uf1d6");
    /// <summary>Official icon: <c>no_meeting_room</c></summary>
    public static string? NoMeetingRoom => Glyph("\ueb4e");
    /// <summary>Official icon: <c>no_photography</c></summary>
    public static string? NoPhotography => Glyph("\uf1a8");
    /// <summary>Official icon: <c>no_sim</c></summary>
    public static string? NoSim => Glyph("\ue1ce");
    /// <summary>Official icon: <c>no_sound</c></summary>
    public static string? NoSound => Glyph("\ue710");
    /// <summary>Official icon: <c>no_stroller</c></summary>
    public static string? NoStroller => Glyph("\uf1af");
    /// <summary>Official icon: <c>no_transfer</c></summary>
    public static string? NoTransfer => Glyph("\uf1d5");
    /// <summary>Official icon: <c>noise_aware</c></summary>
    public static string? NoiseAware => Glyph("\uebec");
    /// <summary>Official icon: <c>noise_control_off</c></summary>
    public static string? NoiseControlOff => Glyph("\uebf3");
    /// <summary>Official icon: <c>noise_control_on</c></summary>
    public static string? NoiseControlOn => Glyph("\uf8a8");
    /// <summary>Official icon: <c>nordic_walking</c></summary>
    public static string? NordicWalking => Glyph("\ue50e");
    /// <summary>Official icon: <c>north</c></summary>
    public static string? North => Glyph("\uf1e0");
    /// <summary>Official icon: <c>north_east</c></summary>
    public static string? NorthEast => Glyph("\uf1e1");
    /// <summary>Official icon: <c>north_west</c></summary>
    public static string? NorthWest => Glyph("\uf1e2");
    /// <summary>Official icon: <c>not_accessible</c></summary>
    public static string? NotAccessible => Glyph("\uf0fe");
    /// <summary>Official icon: <c>not_accessible_forward</c></summary>
    public static string? NotAccessibleForward => Glyph("\uf54a");
    /// <summary>Official icon: <c>not_interested</c></summary>
    public static string? NotInterested => Glyph("\uf08c");
    /// <summary>Official icon: <c>not_listed_location</c></summary>
    public static string? NotListedLocation => Glyph("\ue575");
    /// <summary>Official icon: <c>not_started</c></summary>
    public static string? NotStarted => Glyph("\uf0d1");
    /// <summary>Official icon: <c>note</c></summary>
    public static string? Note => Glyph("\ue66d");
    /// <summary>Official icon: <c>note_add</c></summary>
    public static string? NoteAdd => Glyph("\ue89c");
    /// <summary>Official icon: <c>note_alt</c></summary>
    public static string? NoteAlt => Glyph("\uf040");
    /// <summary>Official icon: <c>note_stack</c></summary>
    public static string? NoteStack => Glyph("\uf562");
    /// <summary>Official icon: <c>note_stack_add</c></summary>
    public static string? NoteStackAdd => Glyph("\uf563");
    /// <summary>Official icon: <c>notes</c></summary>
    public static string? Notes => Glyph("\ue26c");
    /// <summary>Official icon: <c>notification_add</c></summary>
    public static string? NotificationAdd => Glyph("\ue399");
    /// <summary>Official icon: <c>notification_audio</c></summary>
    public static string? NotificationAudio => Glyph("\ueec1");
    /// <summary>Official icon: <c>notification_audio_off</c></summary>
    public static string? NotificationAudioOff => Glyph("\ueec0");
    /// <summary>Official icon: <c>notification_important</c></summary>
    public static string? NotificationImportant => Glyph("\ue004");
    /// <summary>Official icon: <c>notification_multiple</c></summary>
    public static string? NotificationMultiple => Glyph("\ue6c2");
    /// <summary>Official icon: <c>notification_settings</c></summary>
    public static string? NotificationSettings => Glyph("\uf367");
    /// <summary>Official icon: <c>notification_sound</c></summary>
    public static string? NotificationSound => Glyph("\uf353");
    /// <summary>Official icon: <c>notifications</c></summary>
    public static string? Notifications => Glyph("\ue7f5");
    /// <summary>Official icon: <c>notifications_active</c></summary>
    public static string? NotificationsActive => Glyph("\ue7f7");
    /// <summary>Official icon: <c>notifications_none</c></summary>
    public static string? NotificationsNone => Glyph("\ue7f5");
    /// <summary>Official icon: <c>notifications_off</c></summary>
    public static string? NotificationsOff => Glyph("\ue7f6");
    /// <summary>Official icon: <c>notifications_paused</c></summary>
    public static string? NotificationsPaused => Glyph("\ue7f8");
    /// <summary>Official icon: <c>notifications_unread</c></summary>
    public static string? NotificationsUnread => Glyph("\uf4fe");
    /// <summary>Official icon: <c>numbers</c></summary>
    public static string? Numbers => Glyph("\ueac7");
    /// <summary>Official icon: <c>nutrition</c></summary>
    public static string? Nutrition => Glyph("\ue110");
    /// <summary>Official icon: <c>ods</c></summary>
    public static string? Ods => Glyph("\ue6e8");
    /// <summary>Official icon: <c>odt</c></summary>
    public static string? Odt => Glyph("\ue6e9");
    /// <summary>Official icon: <c>offline_bolt</c></summary>
    public static string? OfflineBolt => Glyph("\ue932");
    /// <summary>Official icon: <c>offline_pin</c></summary>
    public static string? OfflinePin => Glyph("\ue90a");
    /// <summary>Official icon: <c>offline_pin_off</c></summary>
    public static string? OfflinePinOff => Glyph("\uf4d0");
    /// <summary>Official icon: <c>offline_share</c></summary>
    public static string? OfflineShare => Glyph("\uf2de");
    /// <summary>Official icon: <c>oil_barrel</c></summary>
    public static string? OilBarrel => Glyph("\uec15");
    /// <summary>Official icon: <c>okonomiyaki</c></summary>
    public static string? Okonomiyaki => Glyph("\uf281");
    /// <summary>Official icon: <c>on_device_training</c></summary>
    public static string? OnDeviceTraining => Glyph("\uebfd");
    /// <summary>Official icon: <c>on_hub_device</c></summary>
    public static string? OnHubDevice => Glyph("\ue6c3");
    /// <summary>Official icon: <c>oncology</c></summary>
    public static string? Oncology => Glyph("\ue114");
    /// <summary>Official icon: <c>ondemand_video</c></summary>
    public static string? OndemandVideo => Glyph("\ue63a");
    /// <summary>Official icon: <c>online_prediction</c></summary>
    public static string? OnlinePrediction => Glyph("\uf0eb");
    /// <summary>Official icon: <c>onsen</c></summary>
    public static string? Onsen => Glyph("\uf6f8");
    /// <summary>Official icon: <c>opacity</c></summary>
    public static string? Opacity => Glyph("\ue91c");
    /// <summary>Official icon: <c>open_in_browser</c></summary>
    public static string? OpenInBrowser => Glyph("\ue89d");
    /// <summary>Official icon: <c>open_in_full</c></summary>
    public static string? OpenInFull => Glyph("\uf1ce");
    /// <summary>Official icon: <c>open_in_new</c></summary>
    public static string? OpenInNew => Glyph("\ue89e");
    /// <summary>Official icon: <c>open_in_new_down</c></summary>
    public static string? OpenInNewDown => Glyph("\uf70f");
    /// <summary>Official icon: <c>open_in_new_off</c></summary>
    public static string? OpenInNewOff => Glyph("\ue4f6");
    /// <summary>Official icon: <c>open_in_phone</c></summary>
    public static string? OpenInPhone => Glyph("\uf2d2");
    /// <summary>Official icon: <c>open_jam</c></summary>
    public static string? OpenJam => Glyph("\uefae");
    /// <summary>Official icon: <c>open_run</c></summary>
    public static string? OpenRun => Glyph("\uf4b7");
    /// <summary>Official icon: <c>open_with</c></summary>
    public static string? OpenWith => Glyph("\ue89f");
    /// <summary>Official icon: <c>ophthalmology</c></summary>
    public static string? Ophthalmology => Glyph("\ue115");
    /// <summary>Official icon: <c>oral_disease</c></summary>
    public static string? OralDisease => Glyph("\ue116");
    /// <summary>Official icon: <c>orbit</c></summary>
    public static string? Orbit => Glyph("\uf426");
    /// <summary>Official icon: <c>order_approve</c></summary>
    public static string? OrderApprove => Glyph("\uf812");
    /// <summary>Official icon: <c>order_play</c></summary>
    public static string? OrderPlay => Glyph("\uf811");
    /// <summary>Official icon: <c>orders</c></summary>
    public static string? Orders => Glyph("\ueb14");
    /// <summary>Official icon: <c>orthopedics</c></summary>
    public static string? Orthopedics => Glyph("\uf897");
    /// <summary>Official icon: <c>other_admission</c></summary>
    public static string? OtherAdmission => Glyph("\ue47b");
    /// <summary>Official icon: <c>other_houses</c></summary>
    public static string? OtherHouses => Glyph("\ue58c");
    /// <summary>Official icon: <c>outbound</c></summary>
    public static string? Outbound => Glyph("\ue1ca");
    /// <summary>Official icon: <c>outbox</c></summary>
    public static string? Outbox => Glyph("\uef5f");
    /// <summary>Official icon: <c>outbox_alt</c></summary>
    public static string? OutboxAlt => Glyph("\ueb17");
    /// <summary>Official icon: <c>outdoor_garden</c></summary>
    public static string? OutdoorGarden => Glyph("\ue205");
    /// <summary>Official icon: <c>outdoor_grill</c></summary>
    public static string? OutdoorGrill => Glyph("\uea47");
    /// <summary>Official icon: <c>outgoing_mail</c></summary>
    public static string? OutgoingMail => Glyph("\uf0d2");
    /// <summary>Official icon: <c>outlet</c></summary>
    public static string? Outlet => Glyph("\uf1d4");
    /// <summary>Official icon: <c>outlined_flag</c></summary>
    public static string? OutlinedFlag => Glyph("\uf0c6");
    /// <summary>Official icon: <c>outpatient</c></summary>
    public static string? Outpatient => Glyph("\ue118");
    /// <summary>Official icon: <c>outpatient_med</c></summary>
    public static string? OutpatientMed => Glyph("\ue119");
    /// <summary>Official icon: <c>output</c></summary>
    public static string? Output => Glyph("\uebbe");
    /// <summary>Official icon: <c>output_circle</c></summary>
    public static string? OutputCircle => Glyph("\uf70e");
    /// <summary>Official icon: <c>oven</c></summary>
    public static string? Oven => Glyph("\ue9c7");
    /// <summary>Official icon: <c>oven_gen</c></summary>
    public static string? OvenGen => Glyph("\ue843");
    /// <summary>Official icon: <c>overview</c></summary>
    public static string? Overview => Glyph("\ue4a7");
    /// <summary>Official icon: <c>overview_key</c></summary>
    public static string? OverviewKey => Glyph("\uf7d4");
    /// <summary>Official icon: <c>owl</c></summary>
    public static string? Owl => Glyph("\uf3b4");
    /// <summary>Official icon: <c>oxygen_saturation</c></summary>
    public static string? OxygenSaturation => Glyph("\ue4de");
    /// <summary>Official icon: <c>p2p</c></summary>
    public static string? P2p => Glyph("\uf52a");
    /// <summary>Official icon: <c>pace</c></summary>
    public static string? Pace => Glyph("\uf6b8");
    /// <summary>Official icon: <c>pacemaker</c></summary>
    public static string? Pacemaker => Glyph("\ue656");
    /// <summary>Official icon: <c>package</c></summary>
    public static string? Package => Glyph("\ue48f");
    /// <summary>Official icon: <c>package_2</c></summary>
    public static string? Package2 => Glyph("\uf569");
    /// <summary>Official icon: <c>padding</c></summary>
    public static string? Padding => Glyph("\ue9c8");
    /// <summary>Official icon: <c>padel</c></summary>
    public static string? Padel => Glyph("\uf2a7");
    /// <summary>Official icon: <c>page_control</c></summary>
    public static string? PageControl => Glyph("\ue731");
    /// <summary>Official icon: <c>page_footer</c></summary>
    public static string? PageFooter => Glyph("\uf383");
    /// <summary>Official icon: <c>page_header</c></summary>
    public static string? PageHeader => Glyph("\uf384");
    /// <summary>Official icon: <c>page_info</c></summary>
    public static string? PageInfo => Glyph("\uf614");
    /// <summary>Official icon: <c>page_menu_ios</c></summary>
    public static string? PageMenuIos => Glyph("\ueefb");
    /// <summary>Official icon: <c>pageless</c></summary>
    public static string? Pageless => Glyph("\uf509");
    /// <summary>Official icon: <c>pages</c></summary>
    public static string? Pages => Glyph("\ue7f9");
    /// <summary>Official icon: <c>pageview</c></summary>
    public static string? Pageview => Glyph("\ue8a0");
    /// <summary>Official icon: <c>paid</c></summary>
    public static string? Paid => Glyph("\uf041");
    /// <summary>Official icon: <c>palette</c></summary>
    public static string? Palette => Glyph("\ue40a");
    /// <summary>Official icon: <c>pallet</c></summary>
    public static string? Pallet => Glyph("\uf86a");
    /// <summary>Official icon: <c>pan_tool</c></summary>
    public static string? PanTool => Glyph("\ue925");
    /// <summary>Official icon: <c>pan_tool_alt</c></summary>
    public static string? PanToolAlt => Glyph("\uebb9");
    /// <summary>Official icon: <c>pan_zoom</c></summary>
    public static string? PanZoom => Glyph("\uf655");
    /// <summary>Official icon: <c>panorama</c></summary>
    public static string? Panorama => Glyph("\ue691");
    /// <summary>Official icon: <c>panorama_fish_eye</c></summary>
    public static string? PanoramaFishEye => Glyph("\ue40c");
    /// <summary>Official icon: <c>panorama_horizontal</c></summary>
    public static string? PanoramaHorizontal => Glyph("\ue40d");
    /// <summary>Official icon: <c>panorama_photosphere</c></summary>
    public static string? PanoramaPhotosphere => Glyph("\ue9c9");
    /// <summary>Official icon: <c>panorama_vertical</c></summary>
    public static string? PanoramaVertical => Glyph("\ue40e");
    /// <summary>Official icon: <c>panorama_wide_angle</c></summary>
    public static string? PanoramaWideAngle => Glyph("\ue40f");
    /// <summary>Official icon: <c>paragliding</c></summary>
    public static string? Paragliding => Glyph("\ue50f");
    /// <summary>Official icon: <c>parent_child_dining</c></summary>
    public static string? ParentChildDining => Glyph("\uf22d");
    /// <summary>Official icon: <c>park</c></summary>
    public static string? Park => Glyph("\uea63");
    /// <summary>Official icon: <c>parking_meter</c></summary>
    public static string? ParkingMeter => Glyph("\uf28a");
    /// <summary>Official icon: <c>parking_sign</c></summary>
    public static string? ParkingSign => Glyph("\uf289");
    /// <summary>Official icon: <c>parking_valet</c></summary>
    public static string? ParkingValet => Glyph("\uf288");
    /// <summary>Official icon: <c>partly_cloudy_day</c></summary>
    public static string? PartlyCloudyDay => Glyph("\uf172");
    /// <summary>Official icon: <c>partly_cloudy_night</c></summary>
    public static string? PartlyCloudyNight => Glyph("\uf174");
    /// <summary>Official icon: <c>partner_exchange</c></summary>
    public static string? PartnerExchange => Glyph("\uf7f9");
    /// <summary>Official icon: <c>partner_heart</c></summary>
    public static string? PartnerHeart => Glyph("\uef2e");
    /// <summary>Official icon: <c>partner_reports</c></summary>
    public static string? PartnerReports => Glyph("\uefaf");
    /// <summary>Official icon: <c>party_mode</c></summary>
    public static string? PartyMode => Glyph("\ue7fa");
    /// <summary>Official icon: <c>passkey</c></summary>
    public static string? Passkey => Glyph("\uf87f");
    /// <summary>Official icon: <c>passport</c></summary>
    public static string? Passport => Glyph("\ueec4");
    /// <summary>Official icon: <c>password</c></summary>
    public static string? Password => Glyph("\uf042");
    /// <summary>Official icon: <c>password_2</c></summary>
    public static string? Password2 => Glyph("\uf4a9");
    /// <summary>Official icon: <c>password_2_off</c></summary>
    public static string? Password2Off => Glyph("\uf4a8");
    /// <summary>Official icon: <c>patient_list</c></summary>
    public static string? PatientList => Glyph("\ue653");
    /// <summary>Official icon: <c>pattern</c></summary>
    public static string? Pattern => Glyph("\uf043");
    /// <summary>Official icon: <c>pause</c></summary>
    public static string? Pause => Glyph("\ue034");
    /// <summary>Official icon: <c>pause_circle</c></summary>
    public static string? PauseCircle => Glyph("\ue1a2");
    /// <summary>Official icon: <c>pause_circle_filled</c></summary>
    public static string? PauseCircleFilled => Glyph("\ue1a2");
    /// <summary>Official icon: <c>pause_circle_outline</c></summary>
    public static string? PauseCircleOutline => Glyph("\ue1a2");
    /// <summary>Official icon: <c>pause_presentation</c></summary>
    public static string? PausePresentation => Glyph("\ue0ea");
    /// <summary>Official icon: <c>payment</c></summary>
    public static string? Payment => Glyph("\ue8a1");
    /// <summary>Official icon: <c>payment_arrow_down</c></summary>
    public static string? PaymentArrowDown => Glyph("\uf2c0");
    /// <summary>Official icon: <c>payment_card</c></summary>
    public static string? PaymentCard => Glyph("\uf2a1");
    /// <summary>Official icon: <c>payments</c></summary>
    public static string? Payments => Glyph("\uef63");
    /// <summary>Official icon: <c>pedal_bike</c></summary>
    public static string? PedalBike => Glyph("\ueb29");
    /// <summary>Official icon: <c>pediatrics</c></summary>
    public static string? Pediatrics => Glyph("\ue11d");
    /// <summary>Official icon: <c>pen_size_1</c></summary>
    public static string? PenSize1 => Glyph("\uf755");
    /// <summary>Official icon: <c>pen_size_2</c></summary>
    public static string? PenSize2 => Glyph("\uf754");
    /// <summary>Official icon: <c>pen_size_3</c></summary>
    public static string? PenSize3 => Glyph("\uf753");
    /// <summary>Official icon: <c>pen_size_4</c></summary>
    public static string? PenSize4 => Glyph("\uf752");
    /// <summary>Official icon: <c>pen_size_5</c></summary>
    public static string? PenSize5 => Glyph("\uf751");
    /// <summary>Official icon: <c>pending</c></summary>
    public static string? Pending => Glyph("\uef64");
    /// <summary>Official icon: <c>pending_actions</c></summary>
    public static string? PendingActions => Glyph("\uf1bb");
    /// <summary>Official icon: <c>pentagon</c></summary>
    public static string? Pentagon => Glyph("\ueb50");
    /// <summary>Official icon: <c>people</c></summary>
    public static string? People => Glyph("\uea21");
    /// <summary>Official icon: <c>people_alt</c></summary>
    public static string? PeopleAlt => Glyph("\uea21");
    /// <summary>Official icon: <c>people_outline</c></summary>
    public static string? PeopleOutline => Glyph("\uea21");
    /// <summary>Official icon: <c>people_size_decrease</c></summary>
    public static string? PeopleSizeDecrease => Glyph("\U000FFEB1");
    /// <summary>Official icon: <c>people_size_increase</c></summary>
    public static string? PeopleSizeIncrease => Glyph("\U000FFEB0");
    /// <summary>Official icon: <c>percent</c></summary>
    public static string? Percent => Glyph("\ueb58");
    /// <summary>Official icon: <c>percent_discount</c></summary>
    public static string? PercentDiscount => Glyph("\uf244");
    /// <summary>Official icon: <c>performance_max</c></summary>
    public static string? PerformanceMax => Glyph("\ue51a");
    /// <summary>Official icon: <c>pergola</c></summary>
    public static string? Pergola => Glyph("\ue203");
    /// <summary>Official icon: <c>perm_camera_mic</c></summary>
    public static string? PermCameraMic => Glyph("\ue8a2");
    /// <summary>Official icon: <c>perm_contact_calendar</c></summary>
    public static string? PermContactCalendar => Glyph("\ue8a3");
    /// <summary>Official icon: <c>perm_data_setting</c></summary>
    public static string? PermDataSetting => Glyph("\ue8a4");
    /// <summary>Official icon: <c>perm_device_information</c></summary>
    public static string? PermDeviceInformation => Glyph("\uf2dc");
    /// <summary>Official icon: <c>perm_identity</c></summary>
    public static string? PermIdentity => Glyph("\uf0d3");
    /// <summary>Official icon: <c>perm_media</c></summary>
    public static string? PermMedia => Glyph("\ue8a7");
    /// <summary>Official icon: <c>perm_phone_msg</c></summary>
    public static string? PermPhoneMsg => Glyph("\ue8a8");
    /// <summary>Official icon: <c>perm_scan_wifi</c></summary>
    public static string? PermScanWifi => Glyph("\ue8a9");
    /// <summary>Official icon: <c>person</c></summary>
    public static string? Person => Glyph("\uf0d3");
    /// <summary>Official icon: <c>person_2</c></summary>
    public static string? Person2 => Glyph("\uf8e4");
    /// <summary>Official icon: <c>person_3</c></summary>
    public static string? Person3 => Glyph("\uf8e5");
    /// <summary>Official icon: <c>person_4</c></summary>
    public static string? Person4 => Glyph("\uf8e6");
    /// <summary>Official icon: <c>person_add</c></summary>
    public static string? PersonAdd => Glyph("\uea4d");
    /// <summary>Official icon: <c>person_add_alt</c></summary>
    public static string? PersonAddAlt => Glyph("\uea4d");
    /// <summary>Official icon: <c>person_add_disabled</c></summary>
    public static string? PersonAddDisabled => Glyph("\ue9cb");
    /// <summary>Official icon: <c>person_alert</c></summary>
    public static string? PersonAlert => Glyph("\uf567");
    /// <summary>Official icon: <c>person_apron</c></summary>
    public static string? PersonApron => Glyph("\uf5a3");
    /// <summary>Official icon: <c>person_book</c></summary>
    public static string? PersonBook => Glyph("\uf5e8");
    /// <summary>Official icon: <c>person_cancel</c></summary>
    public static string? PersonCancel => Glyph("\uf566");
    /// <summary>Official icon: <c>person_celebrate</c></summary>
    public static string? PersonCelebrate => Glyph("\uf7fe");
    /// <summary>Official icon: <c>person_check</c></summary>
    public static string? PersonCheck => Glyph("\uf565");
    /// <summary>Official icon: <c>person_edit</c></summary>
    public static string? PersonEdit => Glyph("\uf4fa");
    /// <summary>Official icon: <c>person_filled</c></summary>
    public static string? PersonFilled => Glyph("\uf0d3");
    /// <summary>Official icon: <c>person_heart</c></summary>
    public static string? PersonHeart => Glyph("\uf290");
    /// <summary>Official icon: <c>person_off</c></summary>
    public static string? PersonOff => Glyph("\ue510");
    /// <summary>Official icon: <c>person_outline</c></summary>
    public static string? PersonOutline => Glyph("\uf0d3");
    /// <summary>Official icon: <c>person_pin</c></summary>
    public static string? PersonPin => Glyph("\ue55a");
    /// <summary>Official icon: <c>person_pin_circle</c></summary>
    public static string? PersonPinCircle => Glyph("\ue56a");
    /// <summary>Official icon: <c>person_play</c></summary>
    public static string? PersonPlay => Glyph("\uf7fd");
    /// <summary>Official icon: <c>person_raised_hand</c></summary>
    public static string? PersonRaisedHand => Glyph("\uf59a");
    /// <summary>Official icon: <c>person_remove</c></summary>
    public static string? PersonRemove => Glyph("\uef66");
    /// <summary>Official icon: <c>person_search</c></summary>
    public static string? PersonSearch => Glyph("\uf106");
    /// <summary>Official icon: <c>person_shield</c></summary>
    public static string? PersonShield => Glyph("\ue384");
    /// <summary>Official icon: <c>person_text</c></summary>
    public static string? PersonText => Glyph("\ueebd");
    /// <summary>Official icon: <c>personal_bag</c></summary>
    public static string? PersonalBag => Glyph("\ueb0e");
    /// <summary>Official icon: <c>personal_bag_off</c></summary>
    public static string? PersonalBagOff => Glyph("\ueb0f");
    /// <summary>Official icon: <c>personal_bag_question</c></summary>
    public static string? PersonalBagQuestion => Glyph("\ueb10");
    /// <summary>Official icon: <c>personal_injury</c></summary>
    public static string? PersonalInjury => Glyph("\ue6da");
    /// <summary>Official icon: <c>personal_places</c></summary>
    public static string? PersonalPlaces => Glyph("\ue703");
    /// <summary>Official icon: <c>personal_video</c></summary>
    public static string? PersonalVideo => Glyph("\ue63b");
    /// <summary>Official icon: <c>pest_control</c></summary>
    public static string? PestControl => Glyph("\uf0fa");
    /// <summary>Official icon: <c>pest_control_rodent</c></summary>
    public static string? PestControlRodent => Glyph("\uf0fd");
    /// <summary>Official icon: <c>pet_supplies</c></summary>
    public static string? PetSupplies => Glyph("\uefb1");
    /// <summary>Official icon: <c>pets</c></summary>
    public static string? Pets => Glyph("\ue91d");
    /// <summary>Official icon: <c>phishing</c></summary>
    public static string? Phishing => Glyph("\uead7");
    /// <summary>Official icon: <c>phone</c></summary>
    public static string? Phone => Glyph("\uf0d4");
    /// <summary>Official icon: <c>phone_alt</c></summary>
    public static string? PhoneAlt => Glyph("\uf0d4");
    /// <summary>Official icon: <c>phone_android</c></summary>
    public static string? PhoneAndroid => Glyph("\uf2db");
    /// <summary>Official icon: <c>phone_bluetooth_speaker</c></summary>
    public static string? PhoneBluetoothSpeaker => Glyph("\ue61b");
    /// <summary>Official icon: <c>phone_callback</c></summary>
    public static string? PhoneCallback => Glyph("\ue649");
    /// <summary>Official icon: <c>phone_cancel</c></summary>
    public static string? PhoneCancel => Glyph("\U000FFF9D");
    /// <summary>Official icon: <c>phone_disabled</c></summary>
    public static string? PhoneDisabled => Glyph("\ue9cc");
    /// <summary>Official icon: <c>phone_enabled</c></summary>
    public static string? PhoneEnabled => Glyph("\ue9cd");
    /// <summary>Official icon: <c>phone_forwarded</c></summary>
    public static string? PhoneForwarded => Glyph("\ue61c");
    /// <summary>Official icon: <c>phone_in_talk</c></summary>
    public static string? PhoneInTalk => Glyph("\ue61d");
    /// <summary>Official icon: <c>phone_iphone</c></summary>
    public static string? PhoneIphone => Glyph("\uf2da");
    /// <summary>Official icon: <c>phone_locked</c></summary>
    public static string? PhoneLocked => Glyph("\ue61e");
    /// <summary>Official icon: <c>phone_missed</c></summary>
    public static string? PhoneMissed => Glyph("\ue61f");
    /// <summary>Official icon: <c>phone_paused</c></summary>
    public static string? PhonePaused => Glyph("\ue620");
    /// <summary>Official icon: <c>phonelink</c></summary>
    public static string? Phonelink => Glyph("\ue326");
    /// <summary>Official icon: <c>phonelink_erase</c></summary>
    public static string? PhonelinkErase => Glyph("\uf2ea");
    /// <summary>Official icon: <c>phonelink_lock</c></summary>
    public static string? PhonelinkLock => Glyph("\uf2be");
    /// <summary>Official icon: <c>phonelink_off</c></summary>
    public static string? PhonelinkOff => Glyph("\uf7a5");
    /// <summary>Official icon: <c>phonelink_ring</c></summary>
    public static string? PhonelinkRing => Glyph("\uf2e8");
    /// <summary>Official icon: <c>phonelink_ring_off</c></summary>
    public static string? PhonelinkRingOff => Glyph("\uf7aa");
    /// <summary>Official icon: <c>phonelink_setup</c></summary>
    public static string? PhonelinkSetup => Glyph("\uf2d9");
    /// <summary>Official icon: <c>photo</c></summary>
    public static string? Photo => Glyph("\ue693");
    /// <summary>Official icon: <c>photo_album</c></summary>
    public static string? PhotoAlbum => Glyph("\ue411");
    /// <summary>Official icon: <c>photo_auto_merge</c></summary>
    public static string? PhotoAutoMerge => Glyph("\uf530");
    /// <summary>Official icon: <c>photo_camera</c></summary>
    public static string? PhotoCamera => Glyph("\ue412");
    /// <summary>Official icon: <c>photo_camera_back</c></summary>
    public static string? PhotoCameraBack => Glyph("\uef68");
    /// <summary>Official icon: <c>photo_camera_front</c></summary>
    public static string? PhotoCameraFront => Glyph("\uef69");
    /// <summary>Official icon: <c>photo_filter</c></summary>
    public static string? PhotoFilter => Glyph("\ue43b");
    /// <summary>Official icon: <c>photo_frame</c></summary>
    public static string? PhotoFrame => Glyph("\uf0d9");
    /// <summary>Official icon: <c>photo_library</c></summary>
    public static string? PhotoLibrary => Glyph("\ue413");
    /// <summary>Official icon: <c>photo_prints</c></summary>
    public static string? PhotoPrints => Glyph("\uefb2");
    /// <summary>Official icon: <c>photo_size_select_actual</c></summary>
    public static string? PhotoSizeSelectActual => Glyph("\ue693");
    /// <summary>Official icon: <c>photo_size_select_large</c></summary>
    public static string? PhotoSizeSelectLarge => Glyph("\ue433");
    /// <summary>Official icon: <c>photo_size_select_small</c></summary>
    public static string? PhotoSizeSelectSmall => Glyph("\ue434");
    /// <summary>Official icon: <c>php</c></summary>
    public static string? Php => Glyph("\ueb8f");
    /// <summary>Official icon: <c>physical_therapy</c></summary>
    public static string? PhysicalTherapy => Glyph("\ue11e");
    /// <summary>Official icon: <c>piano</c></summary>
    public static string? Piano => Glyph("\ue521");
    /// <summary>Official icon: <c>piano_off</c></summary>
    public static string? PianoOff => Glyph("\ue520");
    /// <summary>Official icon: <c>pickleball</c></summary>
    public static string? Pickleball => Glyph("\uf2a6");
    /// <summary>Official icon: <c>picture_as_pdf</c></summary>
    public static string? PictureAsPdf => Glyph("\ue415");
    /// <summary>Official icon: <c>picture_in_picture</c></summary>
    public static string? PictureInPicture => Glyph("\ue8aa");
    /// <summary>Official icon: <c>picture_in_picture_alt</c></summary>
    public static string? PictureInPictureAlt => Glyph("\ue911");
    /// <summary>Official icon: <c>picture_in_picture_center</c></summary>
    public static string? PictureInPictureCenter => Glyph("\uf550");
    /// <summary>Official icon: <c>picture_in_picture_large</c></summary>
    public static string? PictureInPictureLarge => Glyph("\uf54f");
    /// <summary>Official icon: <c>picture_in_picture_medium</c></summary>
    public static string? PictureInPictureMedium => Glyph("\uf54e");
    /// <summary>Official icon: <c>picture_in_picture_mobile</c></summary>
    public static string? PictureInPictureMobile => Glyph("\uf517");
    /// <summary>Official icon: <c>picture_in_picture_off</c></summary>
    public static string? PictureInPictureOff => Glyph("\uf52f");
    /// <summary>Official icon: <c>picture_in_picture_small</c></summary>
    public static string? PictureInPictureSmall => Glyph("\uf54d");
    /// <summary>Official icon: <c>pie_chart</c></summary>
    public static string? PieChart => Glyph("\uf0da");
    /// <summary>Official icon: <c>pie_chart_filled</c></summary>
    public static string? PieChartFilled => Glyph("\uf0da");
    /// <summary>Official icon: <c>pie_chart_outline</c></summary>
    public static string? PieChartOutline => Glyph("\uf0da");
    /// <summary>Official icon: <c>pie_chart_outlined</c></summary>
    public static string? PieChartOutlined => Glyph("\uf0da");
    /// <summary>Official icon: <c>pill</c></summary>
    public static string? Pill => Glyph("\ue11f");
    /// <summary>Official icon: <c>pill_off</c></summary>
    public static string? PillOff => Glyph("\uf809");
    /// <summary>Official icon: <c>pin</c></summary>
    public static string? Pin => Glyph("\uf045");
    /// <summary>Official icon: <c>pin_drop</c></summary>
    public static string? PinDrop => Glyph("\ue55e");
    /// <summary>Official icon: <c>pin_end</c></summary>
    public static string? PinEnd => Glyph("\ue767");
    /// <summary>Official icon: <c>pin_history</c></summary>
    public static string? PinHistory => Glyph("\U000FFF2E");
    /// <summary>Official icon: <c>pin_invoke</c></summary>
    public static string? PinInvoke => Glyph("\ue763");
    /// <summary>Official icon: <c>pin_road</c></summary>
    public static string? PinRoad => Glyph("\U000FFF2D");
    /// <summary>Official icon: <c>pin_road_2</c></summary>
    public static string? PinRoad2 => Glyph("\U000FFEEB");
    /// <summary>Official icon: <c>pinboard</c></summary>
    public static string? Pinboard => Glyph("\uf3ab");
    /// <summary>Official icon: <c>pinboard_unread</c></summary>
    public static string? PinboardUnread => Glyph("\uf3ac");
    /// <summary>Official icon: <c>pinch</c></summary>
    public static string? Pinch => Glyph("\ueb38");
    /// <summary>Official icon: <c>pinch_zoom_in</c></summary>
    public static string? PinchZoomIn => Glyph("\uf1fa");
    /// <summary>Official icon: <c>pinch_zoom_out</c></summary>
    public static string? PinchZoomOut => Glyph("\uf1fb");
    /// <summary>Official icon: <c>pip</c></summary>
    public static string? Pip => Glyph("\uf64d");
    /// <summary>Official icon: <c>pip_exit</c></summary>
    public static string? PipExit => Glyph("\uf70d");
    /// <summary>Official icon: <c>pivot_table_chart</c></summary>
    public static string? PivotTableChart => Glyph("\ue9ce");
    /// <summary>Official icon: <c>place</c></summary>
    public static string? Place => Glyph("\uf1db");
    /// <summary>Official icon: <c>place_item</c></summary>
    public static string? PlaceItem => Glyph("\uf1f0");
    /// <summary>Official icon: <c>plagiarism</c></summary>
    public static string? Plagiarism => Glyph("\uea5a");
    /// <summary>Official icon: <c>plane_contrails</c></summary>
    public static string? PlaneContrails => Glyph("\uf2ac");
    /// <summary>Official icon: <c>planet</c></summary>
    public static string? Planet => Glyph("\uf387");
    /// <summary>Official icon: <c>planner_banner_ad_pt</c></summary>
    public static string? PlannerBannerAdPt => Glyph("\ue692");
    /// <summary>Official icon: <c>planner_review</c></summary>
    public static string? PlannerReview => Glyph("\ue694");
    /// <summary>Official icon: <c>play_arrow</c></summary>
    public static string? PlayArrow => Glyph("\ue037");
    /// <summary>Official icon: <c>play_circle</c></summary>
    public static string? PlayCircle => Glyph("\ue1c4");
    /// <summary>Official icon: <c>play_disabled</c></summary>
    public static string? PlayDisabled => Glyph("\uef6a");
    /// <summary>Official icon: <c>play_for_work</c></summary>
    public static string? PlayForWork => Glyph("\ue906");
    /// <summary>Official icon: <c>play_lesson</c></summary>
    public static string? PlayLesson => Glyph("\uf047");
    /// <summary>Official icon: <c>play_music</c></summary>
    public static string? PlayMusic => Glyph("\ue6ee");
    /// <summary>Official icon: <c>play_pause</c></summary>
    public static string? PlayPause => Glyph("\uf137");
    /// <summary>Official icon: <c>play_shapes</c></summary>
    public static string? PlayShapes => Glyph("\uf7fc");
    /// <summary>Official icon: <c>playground</c></summary>
    public static string? Playground => Glyph("\uf28e");
    /// <summary>Official icon: <c>playground_2</c></summary>
    public static string? Playground2 => Glyph("\uf28f");
    /// <summary>Official icon: <c>playing_cards</c></summary>
    public static string? PlayingCards => Glyph("\uf5dc");
    /// <summary>Official icon: <c>playlist_add</c></summary>
    public static string? PlaylistAdd => Glyph("\ue03b");
    /// <summary>Official icon: <c>playlist_add_check</c></summary>
    public static string? PlaylistAddCheck => Glyph("\ue065");
    /// <summary>Official icon: <c>playlist_add_check_circle</c></summary>
    public static string? PlaylistAddCheckCircle => Glyph("\ue7e6");
    /// <summary>Official icon: <c>playlist_add_circle</c></summary>
    public static string? PlaylistAddCircle => Glyph("\ue7e5");
    /// <summary>Official icon: <c>playlist_play</c></summary>
    public static string? PlaylistPlay => Glyph("\ue05f");
    /// <summary>Official icon: <c>playlist_remove</c></summary>
    public static string? PlaylistRemove => Glyph("\ueb80");
    /// <summary>Official icon: <c>plug_connect</c></summary>
    public static string? PlugConnect => Glyph("\uf35a");
    /// <summary>Official icon: <c>plumbing</c></summary>
    public static string? Plumbing => Glyph("\uf107");
    /// <summary>Official icon: <c>plus_one</c></summary>
    public static string? PlusOne => Glyph("\ue800");
    /// <summary>Official icon: <c>podcasts</c></summary>
    public static string? Podcasts => Glyph("\uf048");
    /// <summary>Official icon: <c>podiatry</c></summary>
    public static string? Podiatry => Glyph("\ue120");
    /// <summary>Official icon: <c>podium</c></summary>
    public static string? Podium => Glyph("\uf7fb");
    /// <summary>Official icon: <c>point_of_sale</c></summary>
    public static string? PointOfSale => Glyph("\uf17e");
    /// <summary>Official icon: <c>point_scan</c></summary>
    public static string? PointScan => Glyph("\uf70c");
    /// <summary>Official icon: <c>poker_chip</c></summary>
    public static string? PokerChip => Glyph("\uf49b");
    /// <summary>Official icon: <c>policy</c></summary>
    public static string? Policy => Glyph("\uea17");
    /// <summary>Official icon: <c>policy_alert</c></summary>
    public static string? PolicyAlert => Glyph("\uf407");
    /// <summary>Official icon: <c>poll</c></summary>
    public static string? Poll => Glyph("\uf0cc");
    /// <summary>Official icon: <c>polyline</c></summary>
    public static string? Polyline => Glyph("\uebbb");
    /// <summary>Official icon: <c>polymer</c></summary>
    public static string? Polymer => Glyph("\ue8ab");
    /// <summary>Official icon: <c>pool</c></summary>
    public static string? Pool => Glyph("\ueb48");
    /// <summary>Official icon: <c>portable_wifi_off</c></summary>
    public static string? PortableWifiOff => Glyph("\uf087");
    /// <summary>Official icon: <c>portrait</c></summary>
    public static string? Portrait => Glyph("\ue851");
    /// <summary>Official icon: <c>position_bottom_left</c></summary>
    public static string? PositionBottomLeft => Glyph("\uf70b");
    /// <summary>Official icon: <c>position_bottom_right</c></summary>
    public static string? PositionBottomRight => Glyph("\uf70a");
    /// <summary>Official icon: <c>position_top_right</c></summary>
    public static string? PositionTopRight => Glyph("\uf709");
    /// <summary>Official icon: <c>post</c></summary>
    public static string? Post => Glyph("\ue705");
    /// <summary>Official icon: <c>post_add</c></summary>
    public static string? PostAdd => Glyph("\uea20");
    /// <summary>Official icon: <c>potted_plant</c></summary>
    public static string? PottedPlant => Glyph("\uf8aa");
    /// <summary>Official icon: <c>power</c></summary>
    public static string? Power => Glyph("\ue63c");
    /// <summary>Official icon: <c>power_input</c></summary>
    public static string? PowerInput => Glyph("\ue336");
    /// <summary>Official icon: <c>power_off</c></summary>
    public static string? PowerOff => Glyph("\ue646");
    /// <summary>Official icon: <c>power_rounded</c></summary>
    public static string? PowerRounded => Glyph("\uf8c7");
    /// <summary>Official icon: <c>power_settings_circle</c></summary>
    public static string? PowerSettingsCircle => Glyph("\uf418");
    /// <summary>Official icon: <c>power_settings_new</c></summary>
    public static string? PowerSettingsNew => Glyph("\uf8c7");
    /// <summary>Official icon: <c>prayer_times</c></summary>
    public static string? PrayerTimes => Glyph("\uf838");
    /// <summary>Official icon: <c>precision_manufacturing</c></summary>
    public static string? PrecisionManufacturing => Glyph("\uf049");
    /// <summary>Official icon: <c>pregnancy</c></summary>
    public static string? Pregnancy => Glyph("\uf5f1");
    /// <summary>Official icon: <c>pregnant_woman</c></summary>
    public static string? PregnantWoman => Glyph("\uf5f1");
    /// <summary>Official icon: <c>preliminary</c></summary>
    public static string? Preliminary => Glyph("\ue7d8");
    /// <summary>Official icon: <c>prescriptions</c></summary>
    public static string? Prescriptions => Glyph("\ue121");
    /// <summary>Official icon: <c>present_to_all</c></summary>
    public static string? PresentToAll => Glyph("\ue0df");
    /// <summary>Official icon: <c>preview</c></summary>
    public static string? Preview => Glyph("\uf1c5");
    /// <summary>Official icon: <c>preview_off</c></summary>
    public static string? PreviewOff => Glyph("\uf7af");
    /// <summary>Official icon: <c>price_change</c></summary>
    public static string? PriceChange => Glyph("\uf04a");
    /// <summary>Official icon: <c>price_check</c></summary>
    public static string? PriceCheck => Glyph("\uf04b");
    /// <summary>Official icon: <c>print</c></summary>
    public static string? Print => Glyph("\ue8ad");
    /// <summary>Official icon: <c>print_add</c></summary>
    public static string? PrintAdd => Glyph("\uf7a2");
    /// <summary>Official icon: <c>print_connect</c></summary>
    public static string? PrintConnect => Glyph("\uf7a1");
    /// <summary>Official icon: <c>print_disabled</c></summary>
    public static string? PrintDisabled => Glyph("\ue9cf");
    /// <summary>Official icon: <c>print_error</c></summary>
    public static string? PrintError => Glyph("\uf7a0");
    /// <summary>Official icon: <c>print_lock</c></summary>
    public static string? PrintLock => Glyph("\uf651");
    /// <summary>Official icon: <c>priority</c></summary>
    public static string? Priority => Glyph("\uefb4");
    /// <summary>Official icon: <c>priority_high</c></summary>
    public static string? PriorityHigh => Glyph("\ue645");
    /// <summary>Official icon: <c>privacy</c></summary>
    public static string? Privacy => Glyph("\uf148");
    /// <summary>Official icon: <c>privacy_tip</c></summary>
    public static string? PrivacyTip => Glyph("\uf0dc");
    /// <summary>Official icon: <c>private_connectivity</c></summary>
    public static string? PrivateConnectivity => Glyph("\ue744");
    /// <summary>Official icon: <c>problem</c></summary>
    public static string? Problem => Glyph("\ue122");
    /// <summary>Official icon: <c>procedure</c></summary>
    public static string? Procedure => Glyph("\ue651");
    /// <summary>Official icon: <c>process_chart</c></summary>
    public static string? ProcessChart => Glyph("\uf855");
    /// <summary>Official icon: <c>production_quantity_limits</c></summary>
    public static string? ProductionQuantityLimits => Glyph("\ue1d1");
    /// <summary>Official icon: <c>productivity</c></summary>
    public static string? Productivity => Glyph("\ue296");
    /// <summary>Official icon: <c>progress_activity</c></summary>
    public static string? ProgressActivity => Glyph("\ue9d0");
    /// <summary>Official icon: <c>prompt_suggestion</c></summary>
    public static string? PromptSuggestion => Glyph("\uf4f6");
    /// <summary>Official icon: <c>propane</c></summary>
    public static string? Propane => Glyph("\uec14");
    /// <summary>Official icon: <c>propane_tank</c></summary>
    public static string? PropaneTank => Glyph("\uec13");
    /// <summary>Official icon: <c>psychiatry</c></summary>
    public static string? Psychiatry => Glyph("\ue123");
    /// <summary>Official icon: <c>psychology</c></summary>
    public static string? Psychology => Glyph("\uea4a");
    /// <summary>Official icon: <c>psychology_alt</c></summary>
    public static string? PsychologyAlt => Glyph("\uf8ea");
    /// <summary>Official icon: <c>public</c></summary>
    public static string? Public => Glyph("\ue80b");
    /// <summary>Official icon: <c>public_off</c></summary>
    public static string? PublicOff => Glyph("\uf1ca");
    /// <summary>Official icon: <c>publish</c></summary>
    public static string? Publish => Glyph("\ue255");
    /// <summary>Official icon: <c>published_with_changes</c></summary>
    public static string? PublishedWithChanges => Glyph("\uf232");
    /// <summary>Official icon: <c>pulmonology</c></summary>
    public static string? Pulmonology => Glyph("\ue124");
    /// <summary>Official icon: <c>pulse_alert</c></summary>
    public static string? PulseAlert => Glyph("\uf501");
    /// <summary>Official icon: <c>punch_clock</c></summary>
    public static string? PunchClock => Glyph("\ueaa8");
    /// <summary>Official icon: <c>push_pin</c></summary>
    public static string? PushPin => Glyph("\uf10d");
    /// <summary>Official icon: <c>qr_code</c></summary>
    public static string? QrCode => Glyph("\uef6b");
    /// <summary>Official icon: <c>qr_code_2</c></summary>
    public static string? QrCode2 => Glyph("\ue00a");
    /// <summary>Official icon: <c>qr_code_2_add</c></summary>
    public static string? QrCode2Add => Glyph("\uf658");
    /// <summary>Official icon: <c>qr_code_scanner</c></summary>
    public static string? QrCodeScanner => Glyph("\uf206");
    /// <summary>Official icon: <c>query_builder</c></summary>
    public static string? QueryBuilder => Glyph("\uefd6");
    /// <summary>Official icon: <c>query_stats</c></summary>
    public static string? QueryStats => Glyph("\ue4fc");
    /// <summary>Official icon: <c>question_answer</c></summary>
    public static string? QuestionAnswer => Glyph("\ue8af");
    /// <summary>Official icon: <c>question_exchange</c></summary>
    public static string? QuestionExchange => Glyph("\uf7f3");
    /// <summary>Official icon: <c>question_mark</c></summary>
    public static string? QuestionMark => Glyph("\ueb8b");
    /// <summary>Official icon: <c>queue</c></summary>
    public static string? Queue => Glyph("\ue03c");
    /// <summary>Official icon: <c>queue_music</c></summary>
    public static string? QueueMusic => Glyph("\ue03d");
    /// <summary>Official icon: <c>queue_play_next</c></summary>
    public static string? QueuePlayNext => Glyph("\ue066");
    /// <summary>Official icon: <c>quick_phrases</c></summary>
    public static string? QuickPhrases => Glyph("\ue7d1");
    /// <summary>Official icon: <c>quick_reference</c></summary>
    public static string? QuickReference => Glyph("\ue46e");
    /// <summary>Official icon: <c>quick_reference_all</c></summary>
    public static string? QuickReferenceAll => Glyph("\uf801");
    /// <summary>Official icon: <c>quick_reorder</c></summary>
    public static string? QuickReorder => Glyph("\ueb15");
    /// <summary>Official icon: <c>quickreply</c></summary>
    public static string? Quickreply => Glyph("\uef6c");
    /// <summary>Official icon: <c>quiet_time</c></summary>
    public static string? QuietTime => Glyph("\uf159");
    /// <summary>Official icon: <c>quiet_time_active</c></summary>
    public static string? QuietTimeActive => Glyph("\ueb76");
    /// <summary>Official icon: <c>quiz</c></summary>
    public static string? Quiz => Glyph("\uf04c");
    /// <summary>Official icon: <c>r_mobiledata</c></summary>
    public static string? RMobiledata => Glyph("\uf04d");
    /// <summary>Official icon: <c>radar</c></summary>
    public static string? Radar => Glyph("\uf04e");
    /// <summary>Official icon: <c>radio</c></summary>
    public static string? Radio => Glyph("\ue03e");
    /// <summary>Official icon: <c>radio_button_checked</c></summary>
    public static string? RadioButtonChecked => Glyph("\ue837");
    /// <summary>Official icon: <c>radio_button_partial</c></summary>
    public static string? RadioButtonPartial => Glyph("\uf560");
    /// <summary>Official icon: <c>radio_button_unchecked</c></summary>
    public static string? RadioButtonUnchecked => Glyph("\ue836");
    /// <summary>Official icon: <c>radiology</c></summary>
    public static string? Radiology => Glyph("\ue125");
    /// <summary>Official icon: <c>railway_alert</c></summary>
    public static string? RailwayAlert => Glyph("\ue9d1");
    /// <summary>Official icon: <c>railway_alert_2</c></summary>
    public static string? RailwayAlert2 => Glyph("\uf461");
    /// <summary>Official icon: <c>rainy</c></summary>
    public static string? Rainy => Glyph("\uf176");
    /// <summary>Official icon: <c>rainy_heavy</c></summary>
    public static string? RainyHeavy => Glyph("\uf61f");
    /// <summary>Official icon: <c>rainy_light</c></summary>
    public static string? RainyLight => Glyph("\uf61e");
    /// <summary>Official icon: <c>rainy_snow</c></summary>
    public static string? RainySnow => Glyph("\uf61d");
    /// <summary>Official icon: <c>ramen_dining</c></summary>
    public static string? RamenDining => Glyph("\uea64");
    /// <summary>Official icon: <c>ramp_left</c></summary>
    public static string? RampLeft => Glyph("\ueb9c");
    /// <summary>Official icon: <c>ramp_right</c></summary>
    public static string? RampRight => Glyph("\ueb96");
    /// <summary>Official icon: <c>range_hood</c></summary>
    public static string? RangeHood => Glyph("\ue1ea");
    /// <summary>Official icon: <c>rate_review</c></summary>
    public static string? RateReview => Glyph("\ue560");
    /// <summary>Official icon: <c>rate_review_rtl</c></summary>
    public static string? RateReviewRtl => Glyph("\ue706");
    /// <summary>Official icon: <c>raven</c></summary>
    public static string? Raven => Glyph("\uf555");
    /// <summary>Official icon: <c>raw_off</c></summary>
    public static string? RawOff => Glyph("\uf04f");
    /// <summary>Official icon: <c>raw_on</c></summary>
    public static string? RawOn => Glyph("\uf050");
    /// <summary>Official icon: <c>read_more</c></summary>
    public static string? ReadMore => Glyph("\uef6d");
    /// <summary>Official icon: <c>readiness_score</c></summary>
    public static string? ReadinessScore => Glyph("\uf6dd");
    /// <summary>Official icon: <c>real_estate_agent</c></summary>
    public static string? RealEstateAgent => Glyph("\ue73a");
    /// <summary>Official icon: <c>rear_camera</c></summary>
    public static string? RearCamera => Glyph("\uf6c2");
    /// <summary>Official icon: <c>rebase</c></summary>
    public static string? Rebase => Glyph("\uf845");
    /// <summary>Official icon: <c>rebase_edit</c></summary>
    public static string? RebaseEdit => Glyph("\uf846");
    /// <summary>Official icon: <c>receipt</c></summary>
    public static string? Receipt => Glyph("\ue8b0");
    /// <summary>Official icon: <c>receipt_long</c></summary>
    public static string? ReceiptLong => Glyph("\uef6e");
    /// <summary>Official icon: <c>receipt_long_off</c></summary>
    public static string? ReceiptLongOff => Glyph("\uf40a");
    /// <summary>Official icon: <c>recent_actors</c></summary>
    public static string? RecentActors => Glyph("\ue03f");
    /// <summary>Official icon: <c>recent_patient</c></summary>
    public static string? RecentPatient => Glyph("\uf808");
    /// <summary>Official icon: <c>recenter</c></summary>
    public static string? Recenter => Glyph("\uf4c0");
    /// <summary>Official icon: <c>recommend</c></summary>
    public static string? Recommend => Glyph("\ue9d2");
    /// <summary>Official icon: <c>record_voice_over</c></summary>
    public static string? RecordVoiceOver => Glyph("\ue91f");
    /// <summary>Official icon: <c>rectangle</c></summary>
    public static string? Rectangle => Glyph("\ueb54");
    /// <summary>Official icon: <c>rectangle_add</c></summary>
    public static string? RectangleAdd => Glyph("\ueec8");
    /// <summary>Official icon: <c>recycling</c></summary>
    public static string? Recycling => Glyph("\ue760");
    /// <summary>Official icon: <c>redeem</c></summary>
    public static string? Redeem => Glyph("\ue8f6");
    /// <summary>Official icon: <c>redo</c></summary>
    public static string? Redo => Glyph("\ue15a");
    /// <summary>Official icon: <c>reduce_capacity</c></summary>
    public static string? ReduceCapacity => Glyph("\uf21c");
    /// <summary>Official icon: <c>refresh</c></summary>
    public static string? Refresh => Glyph("\ue5d5");
    /// <summary>Official icon: <c>regular_expression</c></summary>
    public static string? RegularExpression => Glyph("\uf750");
    /// <summary>Official icon: <c>relax</c></summary>
    public static string? Relax => Glyph("\uf6dc");
    /// <summary>Official icon: <c>release_alert</c></summary>
    public static string? ReleaseAlert => Glyph("\uf654");
    /// <summary>Official icon: <c>remember_me</c></summary>
    public static string? RememberMe => Glyph("\uf051");
    /// <summary>Official icon: <c>reminder</c></summary>
    public static string? Reminder => Glyph("\ue6c6");
    /// <summary>Official icon: <c>reminders_alt</c></summary>
    public static string? RemindersAlt => Glyph("\ue6c6");
    /// <summary>Official icon: <c>remote_gen</c></summary>
    public static string? RemoteGen => Glyph("\ue83e");
    /// <summary>Official icon: <c>remove</c></summary>
    public static string? Remove => Glyph("\ue15b");
    /// <summary>Official icon: <c>remove_circle</c></summary>
    public static string? RemoveCircle => Glyph("\uf08f");
    /// <summary>Official icon: <c>remove_circle_outline</c></summary>
    public static string? RemoveCircleOutline => Glyph("\uf08f");
    /// <summary>Official icon: <c>remove_done</c></summary>
    public static string? RemoveDone => Glyph("\ue9d3");
    /// <summary>Official icon: <c>remove_from_queue</c></summary>
    public static string? RemoveFromQueue => Glyph("\ue067");
    /// <summary>Official icon: <c>remove_moderator</c></summary>
    public static string? RemoveModerator => Glyph("\ue9d4");
    /// <summary>Official icon: <c>remove_red_eye</c></summary>
    public static string? RemoveRedEye => Glyph("\ue8f4");
    /// <summary>Official icon: <c>remove_road</c></summary>
    public static string? RemoveRoad => Glyph("\uebfc");
    /// <summary>Official icon: <c>remove_selection</c></summary>
    public static string? RemoveSelection => Glyph("\ue9d5");
    /// <summary>Official icon: <c>remove_shopping_cart</c></summary>
    public static string? RemoveShoppingCart => Glyph("\ue928");
    /// <summary>Official icon: <c>reopen_window</c></summary>
    public static string? ReopenWindow => Glyph("\uf708");
    /// <summary>Official icon: <c>reorder</c></summary>
    public static string? Reorder => Glyph("\ue8fe");
    /// <summary>Official icon: <c>repartition</c></summary>
    public static string? Repartition => Glyph("\uf8e8");
    /// <summary>Official icon: <c>repeat</c></summary>
    public static string? Repeat => Glyph("\ue040");
    /// <summary>Official icon: <c>repeat_on</c></summary>
    public static string? RepeatOn => Glyph("\ue9d6");
    /// <summary>Official icon: <c>repeat_one</c></summary>
    public static string? RepeatOne => Glyph("\ue041");
    /// <summary>Official icon: <c>repeat_one_on</c></summary>
    public static string? RepeatOneOn => Glyph("\ue9d7");
    /// <summary>Official icon: <c>replace_audio</c></summary>
    public static string? ReplaceAudio => Glyph("\uf451");
    /// <summary>Official icon: <c>replace_image</c></summary>
    public static string? ReplaceImage => Glyph("\uf450");
    /// <summary>Official icon: <c>replace_video</c></summary>
    public static string? ReplaceVideo => Glyph("\uf44f");
    /// <summary>Official icon: <c>replay</c></summary>
    public static string? Replay => Glyph("\ue042");
    /// <summary>Official icon: <c>replay_10</c></summary>
    public static string? Replay10 => Glyph("\ue059");
    /// <summary>Official icon: <c>replay_30</c></summary>
    public static string? Replay30 => Glyph("\ue05a");
    /// <summary>Official icon: <c>replay_5</c></summary>
    public static string? Replay5 => Glyph("\ue05b");
    /// <summary>Official icon: <c>replay_circle_filled</c></summary>
    public static string? ReplayCircleFilled => Glyph("\ue9d8");
    /// <summary>Official icon: <c>reply</c></summary>
    public static string? Reply => Glyph("\ue15e");
    /// <summary>Official icon: <c>reply_all</c></summary>
    public static string? ReplyAll => Glyph("\ue15f");
    /// <summary>Official icon: <c>report</c></summary>
    public static string? Report => Glyph("\uf052");
    /// <summary>Official icon: <c>report_gmailerrorred</c></summary>
    public static string? ReportGmailerrorred => Glyph("\uf052");
    /// <summary>Official icon: <c>report_off</c></summary>
    public static string? ReportOff => Glyph("\ue170");
    /// <summary>Official icon: <c>report_problem</c></summary>
    public static string? ReportProblem => Glyph("\uf083");
    /// <summary>Official icon: <c>request_page</c></summary>
    public static string? RequestPage => Glyph("\uf22c");
    /// <summary>Official icon: <c>request_quote</c></summary>
    public static string? RequestQuote => Glyph("\uf1b6");
    /// <summary>Official icon: <c>reset_brightness</c></summary>
    public static string? ResetBrightness => Glyph("\uf482");
    /// <summary>Official icon: <c>reset_colors</c></summary>
    public static string? ResetColors => Glyph("\U000FFEE2");
    /// <summary>Official icon: <c>reset_exposure</c></summary>
    public static string? ResetExposure => Glyph("\uf266");
    /// <summary>Official icon: <c>reset_focus</c></summary>
    public static string? ResetFocus => Glyph("\uf481");
    /// <summary>Official icon: <c>reset_image</c></summary>
    public static string? ResetImage => Glyph("\uf824");
    /// <summary>Official icon: <c>reset_iso</c></summary>
    public static string? ResetIso => Glyph("\uf480");
    /// <summary>Official icon: <c>reset_settings</c></summary>
    public static string? ResetSettings => Glyph("\uf47f");
    /// <summary>Official icon: <c>reset_shadow</c></summary>
    public static string? ResetShadow => Glyph("\uf47e");
    /// <summary>Official icon: <c>reset_shutter_speed</c></summary>
    public static string? ResetShutterSpeed => Glyph("\uf47d");
    /// <summary>Official icon: <c>reset_tv</c></summary>
    public static string? ResetTv => Glyph("\ue9d9");
    /// <summary>Official icon: <c>reset_white_balance</c></summary>
    public static string? ResetWhiteBalance => Glyph("\uf47c");
    /// <summary>Official icon: <c>reset_wrench</c></summary>
    public static string? ResetWrench => Glyph("\uf56c");
    /// <summary>Official icon: <c>resize</c></summary>
    public static string? Resize => Glyph("\uf707");
    /// <summary>Official icon: <c>resize_window</c></summary>
    public static string? ResizeWindow => Glyph("\U000FFF99");
    /// <summary>Official icon: <c>respiratory_rate</c></summary>
    public static string? RespiratoryRate => Glyph("\ue127");
    /// <summary>Official icon: <c>responsive_layout</c></summary>
    public static string? ResponsiveLayout => Glyph("\ue9da");
    /// <summary>Official icon: <c>rest_area</c></summary>
    public static string? RestArea => Glyph("\uf22a");
    /// <summary>Official icon: <c>restart_alt</c></summary>
    public static string? RestartAlt => Glyph("\uf053");
    /// <summary>Official icon: <c>restaurant</c></summary>
    public static string? Restaurant => Glyph("\ue56c");
    /// <summary>Official icon: <c>restaurant_menu</c></summary>
    public static string? RestaurantMenu => Glyph("\ue561");
    /// <summary>Official icon: <c>restore</c></summary>
    public static string? Restore => Glyph("\ue8b3");
    /// <summary>Official icon: <c>restore_from_trash</c></summary>
    public static string? RestoreFromTrash => Glyph("\ue938");
    /// <summary>Official icon: <c>restore_page</c></summary>
    public static string? RestorePage => Glyph("\ue929");
    /// <summary>Official icon: <c>resume</c></summary>
    public static string? Resume => Glyph("\uf7d0");
    /// <summary>Official icon: <c>reviews</c></summary>
    public static string? Reviews => Glyph("\uf07c");
    /// <summary>Official icon: <c>rewarded_ads</c></summary>
    public static string? RewardedAds => Glyph("\uefb6");
    /// <summary>Official icon: <c>rheumatology</c></summary>
    public static string? Rheumatology => Glyph("\ue128");
    /// <summary>Official icon: <c>rib_cage</c></summary>
    public static string? RibCage => Glyph("\uf898");
    /// <summary>Official icon: <c>rice_bowl</c></summary>
    public static string? RiceBowl => Glyph("\uf1f5");
    /// <summary>Official icon: <c>right_click</c></summary>
    public static string? RightClick => Glyph("\uf706");
    /// <summary>Official icon: <c>right_panel_close</c></summary>
    public static string? RightPanelClose => Glyph("\uf705");
    /// <summary>Official icon: <c>right_panel_open</c></summary>
    public static string? RightPanelOpen => Glyph("\uf704");
    /// <summary>Official icon: <c>ring_volume</c></summary>
    public static string? RingVolume => Glyph("\uf0dd");
    /// <summary>Official icon: <c>ring_volume_filled</c></summary>
    public static string? RingVolumeFilled => Glyph("\uf0dd");
    /// <summary>Official icon: <c>ripples</c></summary>
    public static string? Ripples => Glyph("\ue9db");
    /// <summary>Official icon: <c>road</c></summary>
    public static string? Road => Glyph("\uf472");
    /// <summary>Official icon: <c>robot</c></summary>
    public static string? Robot => Glyph("\uf882");
    /// <summary>Official icon: <c>robot_2</c></summary>
    public static string? Robot2 => Glyph("\uf5d0");
    /// <summary>Official icon: <c>rocket</c></summary>
    public static string? Rocket => Glyph("\ueba5");
    /// <summary>Official icon: <c>rocket_launch</c></summary>
    public static string? RocketLaunch => Glyph("\ueb9b");
    /// <summary>Official icon: <c>roller_shades</c></summary>
    public static string? RollerShades => Glyph("\uec12");
    /// <summary>Official icon: <c>roller_shades_closed</c></summary>
    public static string? RollerShadesClosed => Glyph("\uec11");
    /// <summary>Official icon: <c>roller_skating</c></summary>
    public static string? RollerSkating => Glyph("\uebcd");
    /// <summary>Official icon: <c>roofing</c></summary>
    public static string? Roofing => Glyph("\uf201");
    /// <summary>Official icon: <c>room</c></summary>
    public static string? Room => Glyph("\uf1db");
    /// <summary>Official icon: <c>room_preferences</c></summary>
    public static string? RoomPreferences => Glyph("\uf1b8");
    /// <summary>Official icon: <c>room_service</c></summary>
    public static string? RoomService => Glyph("\ueb49");
    /// <summary>Official icon: <c>rotate_90_degrees_ccw</c></summary>
    public static string? Rotate90DegreesCcw => Glyph("\ue418");
    /// <summary>Official icon: <c>rotate_90_degrees_cw</c></summary>
    public static string? Rotate90DegreesCw => Glyph("\ueaab");
    /// <summary>Official icon: <c>rotate_auto</c></summary>
    public static string? RotateAuto => Glyph("\uf417");
    /// <summary>Official icon: <c>rotate_left</c></summary>
    public static string? RotateLeft => Glyph("\ue419");
    /// <summary>Official icon: <c>rotate_right</c></summary>
    public static string? RotateRight => Glyph("\ue41a");
    /// <summary>Official icon: <c>roundabout_left</c></summary>
    public static string? RoundaboutLeft => Glyph("\ueb99");
    /// <summary>Official icon: <c>roundabout_right</c></summary>
    public static string? RoundaboutRight => Glyph("\ueba3");
    /// <summary>Official icon: <c>rounded_corner</c></summary>
    public static string? RoundedCorner => Glyph("\ue920");
    /// <summary>Official icon: <c>route</c></summary>
    public static string? Route => Glyph("\ueacd");
    /// <summary>Official icon: <c>router</c></summary>
    public static string? Router => Glyph("\ue328");
    /// <summary>Official icon: <c>router_off</c></summary>
    public static string? RouterOff => Glyph("\uf2f4");
    /// <summary>Official icon: <c>routine</c></summary>
    public static string? Routine => Glyph("\ue20c");
    /// <summary>Official icon: <c>rowing</c></summary>
    public static string? Rowing => Glyph("\ue921");
    /// <summary>Official icon: <c>rss_feed</c></summary>
    public static string? RssFeed => Glyph("\ue0e5");
    /// <summary>Official icon: <c>rsvp</c></summary>
    public static string? Rsvp => Glyph("\uf055");
    /// <summary>Official icon: <c>rtt</c></summary>
    public static string? Rtt => Glyph("\ue9ad");
    /// <summary>Official icon: <c>rubric</c></summary>
    public static string? Rubric => Glyph("\ueb27");
    /// <summary>Official icon: <c>rule</c></summary>
    public static string? Rule => Glyph("\uf1c2");
    /// <summary>Official icon: <c>rule_folder</c></summary>
    public static string? RuleFolder => Glyph("\uf1c9");
    /// <summary>Official icon: <c>rule_settings</c></summary>
    public static string? RuleSettings => Glyph("\uf64c");
    /// <summary>Official icon: <c>run_circle</c></summary>
    public static string? RunCircle => Glyph("\uef6f");
    /// <summary>Official icon: <c>running_with_errors</c></summary>
    public static string? RunningWithErrors => Glyph("\ue51d");
    /// <summary>Official icon: <c>rv_hookup</c></summary>
    public static string? RvHookup => Glyph("\ue642");
    /// <summary>Official icon: <c>safety_check</c></summary>
    public static string? SafetyCheck => Glyph("\uebef");
    /// <summary>Official icon: <c>safety_check_off</c></summary>
    public static string? SafetyCheckOff => Glyph("\uf59d");
    /// <summary>Official icon: <c>safety_divider</c></summary>
    public static string? SafetyDivider => Glyph("\ue1cc");
    /// <summary>Official icon: <c>sailing</c></summary>
    public static string? Sailing => Glyph("\ue502");
    /// <summary>Official icon: <c>salinity</c></summary>
    public static string? Salinity => Glyph("\uf876");
    /// <summary>Official icon: <c>sanitizer</c></summary>
    public static string? Sanitizer => Glyph("\uf21d");
    /// <summary>Official icon: <c>satellite</c></summary>
    public static string? Satellite => Glyph("\ue562");
    /// <summary>Official icon: <c>satellite_alt</c></summary>
    public static string? SatelliteAlt => Glyph("\ueb3a");
    /// <summary>Official icon: <c>sauna</c></summary>
    public static string? Sauna => Glyph("\uf6f7");
    /// <summary>Official icon: <c>save</c></summary>
    public static string? Save => Glyph("\ue161");
    /// <summary>Official icon: <c>save_alt</c></summary>
    public static string? SaveAlt => Glyph("\uf090");
    /// <summary>Official icon: <c>save_as</c></summary>
    public static string? SaveAs => Glyph("\ueb60");
    /// <summary>Official icon: <c>save_clock</c></summary>
    public static string? SaveClock => Glyph("\uf398");
    /// <summary>Official icon: <c>saved_search</c></summary>
    public static string? SavedSearch => Glyph("\uea11");
    /// <summary>Official icon: <c>savings</c></summary>
    public static string? Savings => Glyph("\ue2eb");
    /// <summary>Official icon: <c>scale</c></summary>
    public static string? Scale => Glyph("\ueb5f");
    /// <summary>Official icon: <c>scan</c></summary>
    public static string? Scan => Glyph("\uf74e");
    /// <summary>Official icon: <c>scan_delete</c></summary>
    public static string? ScanDelete => Glyph("\uf74f");
    /// <summary>Official icon: <c>scanner</c></summary>
    public static string? Scanner => Glyph("\ue329");
    /// <summary>Official icon: <c>scatter_plot</c></summary>
    public static string? ScatterPlot => Glyph("\ue268");
    /// <summary>Official icon: <c>scene</c></summary>
    public static string? Scene => Glyph("\ue2a7");
    /// <summary>Official icon: <c>schedule</c></summary>
    public static string? Schedule => Glyph("\uefd6");
    /// <summary>Official icon: <c>schedule_send</c></summary>
    public static string? ScheduleSend => Glyph("\uea0a");
    /// <summary>Official icon: <c>schema</c></summary>
    public static string? Schema => Glyph("\ue4fd");
    /// <summary>Official icon: <c>school</c></summary>
    public static string? School => Glyph("\ue80c");
    /// <summary>Official icon: <c>science</c></summary>
    public static string? Science => Glyph("\uea4b");
    /// <summary>Official icon: <c>science_off</c></summary>
    public static string? ScienceOff => Glyph("\uf542");
    /// <summary>Official icon: <c>scooter</c></summary>
    public static string? Scooter => Glyph("\uf471");
    /// <summary>Official icon: <c>score</c></summary>
    public static string? Score => Glyph("\ue269");
    /// <summary>Official icon: <c>scoreboard</c></summary>
    public static string? Scoreboard => Glyph("\uebd0");
    /// <summary>Official icon: <c>screen_lock_landscape</c></summary>
    public static string? ScreenLockLandscape => Glyph("\uf2d8");
    /// <summary>Official icon: <c>screen_lock_portrait</c></summary>
    public static string? ScreenLockPortrait => Glyph("\uf2be");
    /// <summary>Official icon: <c>screen_lock_rotation</c></summary>
    public static string? ScreenLockRotation => Glyph("\uf2d6");
    /// <summary>Official icon: <c>screen_record</c></summary>
    public static string? ScreenRecord => Glyph("\uf679");
    /// <summary>Official icon: <c>screen_rotation</c></summary>
    public static string? ScreenRotation => Glyph("\uf2d5");
    /// <summary>Official icon: <c>screen_rotation_alt</c></summary>
    public static string? ScreenRotationAlt => Glyph("\uebee");
    /// <summary>Official icon: <c>screen_rotation_up</c></summary>
    public static string? ScreenRotationUp => Glyph("\uf678");
    /// <summary>Official icon: <c>screen_search_desktop</c></summary>
    public static string? ScreenSearchDesktop => Glyph("\uef70");
    /// <summary>Official icon: <c>screen_share</c></summary>
    public static string? ScreenShare => Glyph("\ue0e2");
    /// <summary>Official icon: <c>screenshot</c></summary>
    public static string? Screenshot => Glyph("\uf056");
    /// <summary>Official icon: <c>screenshot_frame</c></summary>
    public static string? ScreenshotFrame => Glyph("\uf677");
    /// <summary>Official icon: <c>screenshot_frame_2</c></summary>
    public static string? ScreenshotFrame2 => Glyph("\uf374");
    /// <summary>Official icon: <c>screenshot_keyboard</c></summary>
    public static string? ScreenshotKeyboard => Glyph("\uf7d3");
    /// <summary>Official icon: <c>screenshot_monitor</c></summary>
    public static string? ScreenshotMonitor => Glyph("\uec08");
    /// <summary>Official icon: <c>screenshot_region</c></summary>
    public static string? ScreenshotRegion => Glyph("\uf7d2");
    /// <summary>Official icon: <c>screenshot_tablet</c></summary>
    public static string? ScreenshotTablet => Glyph("\uf697");
    /// <summary>Official icon: <c>script</c></summary>
    public static string? Script => Glyph("\uf45f");
    /// <summary>Official icon: <c>scrollable_header</c></summary>
    public static string? ScrollableHeader => Glyph("\ue9dc");
    /// <summary>Official icon: <c>scuba_diving</c></summary>
    public static string? ScubaDiving => Glyph("\uebce");
    /// <summary>Official icon: <c>sd</c></summary>
    public static string? Sd => Glyph("\ue9dd");
    /// <summary>Official icon: <c>sd_card</c></summary>
    public static string? SdCard => Glyph("\ue623");
    /// <summary>Official icon: <c>sd_card_alert</c></summary>
    public static string? SdCardAlert => Glyph("\uf057");
    /// <summary>Official icon: <c>sd_storage</c></summary>
    public static string? SdStorage => Glyph("\ue623");
    /// <summary>Official icon: <c>sdk</c></summary>
    public static string? Sdk => Glyph("\ue720");
    /// <summary>Official icon: <c>search</c></summary>
    public static string? Search => Glyph("\uef7a");
    /// <summary>Official icon: <c>search_activity</c></summary>
    public static string? SearchActivity => Glyph("\uf3e5");
    /// <summary>Official icon: <c>search_check</c></summary>
    public static string? SearchCheck => Glyph("\uf800");
    /// <summary>Official icon: <c>search_check_2</c></summary>
    public static string? SearchCheck2 => Glyph("\uf469");
    /// <summary>Official icon: <c>search_gear</c></summary>
    public static string? SearchGear => Glyph("\ueefa");
    /// <summary>Official icon: <c>search_hands_free</c></summary>
    public static string? SearchHandsFree => Glyph("\ue696");
    /// <summary>Official icon: <c>search_insights</c></summary>
    public static string? SearchInsights => Glyph("\uf4bc");
    /// <summary>Official icon: <c>search_off</c></summary>
    public static string? SearchOff => Glyph("\uea76");
    /// <summary>Official icon: <c>seat_cool_left</c></summary>
    public static string? SeatCoolLeft => Glyph("\uf331");
    /// <summary>Official icon: <c>seat_cool_right</c></summary>
    public static string? SeatCoolRight => Glyph("\uf330");
    /// <summary>Official icon: <c>seat_heat_left</c></summary>
    public static string? SeatHeatLeft => Glyph("\uf32f");
    /// <summary>Official icon: <c>seat_heat_right</c></summary>
    public static string? SeatHeatRight => Glyph("\uf32e");
    /// <summary>Official icon: <c>seat_read</c></summary>
    public static string? SeatRead => Glyph("\U000FFEDA");
    /// <summary>Official icon: <c>seat_vent_left</c></summary>
    public static string? SeatVentLeft => Glyph("\uf32d");
    /// <summary>Official icon: <c>seat_vent_right</c></summary>
    public static string? SeatVentRight => Glyph("\uf32c");
    /// <summary>Official icon: <c>seat_window</c></summary>
    public static string? SeatWindow => Glyph("\U000FFEE1");
    /// <summary>Official icon: <c>security</c></summary>
    public static string? Security => Glyph("\ue32a");
    /// <summary>Official icon: <c>security_key</c></summary>
    public static string? SecurityKey => Glyph("\uf503");
    /// <summary>Official icon: <c>security_update</c></summary>
    public static string? SecurityUpdate => Glyph("\uf2cd");
    /// <summary>Official icon: <c>security_update_good</c></summary>
    public static string? SecurityUpdateGood => Glyph("\uf073");
    /// <summary>Official icon: <c>security_update_warning</c></summary>
    public static string? SecurityUpdateWarning => Glyph("\uf2d3");
    /// <summary>Official icon: <c>segment</c></summary>
    public static string? Segment => Glyph("\ue94b");
    /// <summary>Official icon: <c>select</c></summary>
    public static string? Select => Glyph("\uf74d");
    /// <summary>Official icon: <c>select_all</c></summary>
    public static string? SelectAll => Glyph("\ue162");
    /// <summary>Official icon: <c>select_check_box</c></summary>
    public static string? SelectCheckBox => Glyph("\uf1fe");
    /// <summary>Official icon: <c>select_to_speak</c></summary>
    public static string? SelectToSpeak => Glyph("\uf7cf");
    /// <summary>Official icon: <c>select_window</c></summary>
    public static string? SelectWindow => Glyph("\ue6fa");
    /// <summary>Official icon: <c>select_window_2</c></summary>
    public static string? SelectWindow2 => Glyph("\uf4c8");
    /// <summary>Official icon: <c>select_window_off</c></summary>
    public static string? SelectWindowOff => Glyph("\ue506");
    /// <summary>Official icon: <c>self_care</c></summary>
    public static string? SelfCare => Glyph("\uf86d");
    /// <summary>Official icon: <c>self_improvement</c></summary>
    public static string? SelfImprovement => Glyph("\uea78");
    /// <summary>Official icon: <c>sell</c></summary>
    public static string? Sell => Glyph("\uf05b");
    /// <summary>Official icon: <c>sell_cloud</c></summary>
    public static string? SellCloud => Glyph("\U000FFF7B");
    /// <summary>Official icon: <c>send</c></summary>
    public static string? Send => Glyph("\ue163");
    /// <summary>Official icon: <c>send_and_archive</c></summary>
    public static string? SendAndArchive => Glyph("\uea0c");
    /// <summary>Official icon: <c>send_money</c></summary>
    public static string? SendMoney => Glyph("\ue8b7");
    /// <summary>Official icon: <c>send_time_extension</c></summary>
    public static string? SendTimeExtension => Glyph("\ueadb");
    /// <summary>Official icon: <c>send_to_mobile</c></summary>
    public static string? SendToMobile => Glyph("\uf2d2");
    /// <summary>Official icon: <c>sensor_door</c></summary>
    public static string? SensorDoor => Glyph("\uf1b5");
    /// <summary>Official icon: <c>sensor_occupied</c></summary>
    public static string? SensorOccupied => Glyph("\uec10");
    /// <summary>Official icon: <c>sensor_window</c></summary>
    public static string? SensorWindow => Glyph("\uf1b4");
    /// <summary>Official icon: <c>sensors</c></summary>
    public static string? Sensors => Glyph("\ue51e");
    /// <summary>Official icon: <c>sensors_krx</c></summary>
    public static string? SensorsKrx => Glyph("\uf556");
    /// <summary>Official icon: <c>sensors_krx_off</c></summary>
    public static string? SensorsKrxOff => Glyph("\uf515");
    /// <summary>Official icon: <c>sensors_off</c></summary>
    public static string? SensorsOff => Glyph("\ue51f");
    /// <summary>Official icon: <c>sentiment_calm</c></summary>
    public static string? SentimentCalm => Glyph("\uf6a7");
    /// <summary>Official icon: <c>sentiment_content</c></summary>
    public static string? SentimentContent => Glyph("\uf6a6");
    /// <summary>Official icon: <c>sentiment_dissatisfied</c></summary>
    public static string? SentimentDissatisfied => Glyph("\ue811");
    /// <summary>Official icon: <c>sentiment_excited</c></summary>
    public static string? SentimentExcited => Glyph("\uf6a5");
    /// <summary>Official icon: <c>sentiment_extremely_dissatisfied</c></summary>
    public static string? SentimentExtremelyDissatisfied => Glyph("\uf194");
    /// <summary>Official icon: <c>sentiment_frustrated</c></summary>
    public static string? SentimentFrustrated => Glyph("\uf6a4");
    /// <summary>Official icon: <c>sentiment_neutral</c></summary>
    public static string? SentimentNeutral => Glyph("\ue812");
    /// <summary>Official icon: <c>sentiment_sad</c></summary>
    public static string? SentimentSad => Glyph("\uf6a3");
    /// <summary>Official icon: <c>sentiment_satisfied</c></summary>
    public static string? SentimentSatisfied => Glyph("\ue813");
    /// <summary>Official icon: <c>sentiment_satisfied_alt</c></summary>
    public static string? SentimentSatisfiedAlt => Glyph("\ue813");
    /// <summary>Official icon: <c>sentiment_stressed</c></summary>
    public static string? SentimentStressed => Glyph("\uf6a2");
    /// <summary>Official icon: <c>sentiment_very_dissatisfied</c></summary>
    public static string? SentimentVeryDissatisfied => Glyph("\ue814");
    /// <summary>Official icon: <c>sentiment_very_satisfied</c></summary>
    public static string? SentimentVerySatisfied => Glyph("\ue815");
    /// <summary>Official icon: <c>sentiment_worried</c></summary>
    public static string? SentimentWorried => Glyph("\uf6a1");
    /// <summary>Official icon: <c>serif</c></summary>
    public static string? Serif => Glyph("\uf4ac");
    /// <summary>Official icon: <c>server_person</c></summary>
    public static string? ServerPerson => Glyph("\uf3bd");
    /// <summary>Official icon: <c>service_toolbox</c></summary>
    public static string? ServiceToolbox => Glyph("\ue717");
    /// <summary>Official icon: <c>set_meal</c></summary>
    public static string? SetMeal => Glyph("\uf1ea");
    /// <summary>Official icon: <c>settings</c></summary>
    public static string? Settings => Glyph("\ue8b8");
    /// <summary>Official icon: <c>settings_accessibility</c></summary>
    public static string? SettingsAccessibility => Glyph("\uf05d");
    /// <summary>Official icon: <c>settings_account_box</c></summary>
    public static string? SettingsAccountBox => Glyph("\uf835");
    /// <summary>Official icon: <c>settings_alert</c></summary>
    public static string? SettingsAlert => Glyph("\uf143");
    /// <summary>Official icon: <c>settings_applications</c></summary>
    public static string? SettingsApplications => Glyph("\ue8b9");
    /// <summary>Official icon: <c>settings_b_roll</c></summary>
    public static string? SettingsBRoll => Glyph("\uf625");
    /// <summary>Official icon: <c>settings_backup_restore</c></summary>
    public static string? SettingsBackupRestore => Glyph("\ue8ba");
    /// <summary>Official icon: <c>settings_bluetooth</c></summary>
    public static string? SettingsBluetooth => Glyph("\ue8bb");
    /// <summary>Official icon: <c>settings_brightness</c></summary>
    public static string? SettingsBrightness => Glyph("\ue8bd");
    /// <summary>Official icon: <c>settings_cell</c></summary>
    public static string? SettingsCell => Glyph("\uf2d1");
    /// <summary>Official icon: <c>settings_cinematic_blur</c></summary>
    public static string? SettingsCinematicBlur => Glyph("\uf624");
    /// <summary>Official icon: <c>settings_ethernet</c></summary>
    public static string? SettingsEthernet => Glyph("\ue8be");
    /// <summary>Official icon: <c>settings_heart</c></summary>
    public static string? SettingsHeart => Glyph("\uf522");
    /// <summary>Official icon: <c>settings_input_antenna</c></summary>
    public static string? SettingsInputAntenna => Glyph("\ue8bf");
    /// <summary>Official icon: <c>settings_input_component</c></summary>
    public static string? SettingsInputComponent => Glyph("\ue8c1");
    /// <summary>Official icon: <c>settings_input_composite</c></summary>
    public static string? SettingsInputComposite => Glyph("\ue8c1");
    /// <summary>Official icon: <c>settings_input_hdmi</c></summary>
    public static string? SettingsInputHdmi => Glyph("\ue8c2");
    /// <summary>Official icon: <c>settings_input_svideo</c></summary>
    public static string? SettingsInputSvideo => Glyph("\ue8c3");
    /// <summary>Official icon: <c>settings_motion_mode</c></summary>
    public static string? SettingsMotionMode => Glyph("\uf833");
    /// <summary>Official icon: <c>settings_night_sight</c></summary>
    public static string? SettingsNightSight => Glyph("\uf832");
    /// <summary>Official icon: <c>settings_overscan</c></summary>
    public static string? SettingsOverscan => Glyph("\ue8c4");
    /// <summary>Official icon: <c>settings_panorama</c></summary>
    public static string? SettingsPanorama => Glyph("\uf831");
    /// <summary>Official icon: <c>settings_phone</c></summary>
    public static string? SettingsPhone => Glyph("\ue8c5");
    /// <summary>Official icon: <c>settings_photo_camera</c></summary>
    public static string? SettingsPhotoCamera => Glyph("\uf834");
    /// <summary>Official icon: <c>settings_power</c></summary>
    public static string? SettingsPower => Glyph("\ue8c6");
    /// <summary>Official icon: <c>settings_remote</c></summary>
    public static string? SettingsRemote => Glyph("\ue8c7");
    /// <summary>Official icon: <c>settings_screen</c></summary>
    public static string? SettingsScreen => Glyph("\U000FFEDF");
    /// <summary>Official icon: <c>settings_seating</c></summary>
    public static string? SettingsSeating => Glyph("\uef2d");
    /// <summary>Official icon: <c>settings_slow_motion</c></summary>
    public static string? SettingsSlowMotion => Glyph("\uf623");
    /// <summary>Official icon: <c>settings_suggest</c></summary>
    public static string? SettingsSuggest => Glyph("\uf05e");
    /// <summary>Official icon: <c>settings_system_daydream</c></summary>
    public static string? SettingsSystemDaydream => Glyph("\ue1c3");
    /// <summary>Official icon: <c>settings_timelapse</c></summary>
    public static string? SettingsTimelapse => Glyph("\uf622");
    /// <summary>Official icon: <c>settings_video_camera</c></summary>
    public static string? SettingsVideoCamera => Glyph("\uf621");
    /// <summary>Official icon: <c>settings_voice</c></summary>
    public static string? SettingsVoice => Glyph("\ue8c8");
    /// <summary>Official icon: <c>settop_component</c></summary>
    public static string? SettopComponent => Glyph("\ue2ac");
    /// <summary>Official icon: <c>severe_cold</c></summary>
    public static string? SevereCold => Glyph("\uebd3");
    /// <summary>Official icon: <c>shades</c></summary>
    public static string? Shades => Glyph("\U000FFF73");
    /// <summary>Official icon: <c>shades_closed</c></summary>
    public static string? ShadesClosed => Glyph("\U000FFF74");
    /// <summary>Official icon: <c>shadow</c></summary>
    public static string? Shadow => Glyph("\ue9df");
    /// <summary>Official icon: <c>shadow_add</c></summary>
    public static string? ShadowAdd => Glyph("\uf584");
    /// <summary>Official icon: <c>shadow_minus</c></summary>
    public static string? ShadowMinus => Glyph("\uf583");
    /// <summary>Official icon: <c>shape_line</c></summary>
    public static string? ShapeLine => Glyph("\uf8d3");
    /// <summary>Official icon: <c>shape_recognition</c></summary>
    public static string? ShapeRecognition => Glyph("\ueb01");
    /// <summary>Official icon: <c>shapes</c></summary>
    public static string? Shapes => Glyph("\ue602");
    /// <summary>Official icon: <c>share</c></summary>
    public static string? Share => Glyph("\ue80d");
    /// <summary>Official icon: <c>share_eta</c></summary>
    public static string? ShareEta => Glyph("\ue5f7");
    /// <summary>Official icon: <c>share_location</c></summary>
    public static string? ShareLocation => Glyph("\uf05f");
    /// <summary>Official icon: <c>share_off</c></summary>
    public static string? ShareOff => Glyph("\uf6cb");
    /// <summary>Official icon: <c>share_reviews</c></summary>
    public static string? ShareReviews => Glyph("\uf8a4");
    /// <summary>Official icon: <c>share_windows</c></summary>
    public static string? ShareWindows => Glyph("\uf613");
    /// <summary>Official icon: <c>shaved_ice</c></summary>
    public static string? ShavedIce => Glyph("\uf225");
    /// <summary>Official icon: <c>sheets_rtl</c></summary>
    public static string? SheetsRtl => Glyph("\uf823");
    /// <summary>Official icon: <c>shelf_auto_hide</c></summary>
    public static string? ShelfAutoHide => Glyph("\uf703");
    /// <summary>Official icon: <c>shelf_position</c></summary>
    public static string? ShelfPosition => Glyph("\uf702");
    /// <summary>Official icon: <c>shelves</c></summary>
    public static string? Shelves => Glyph("\uf86e");
    /// <summary>Official icon: <c>shield</c></summary>
    public static string? Shield => Glyph("\ue9e0");
    /// <summary>Official icon: <c>shield_card</c></summary>
    public static string? ShieldCard => Glyph("\U000FFF30");
    /// <summary>Official icon: <c>shield_lock</c></summary>
    public static string? ShieldLock => Glyph("\uf686");
    /// <summary>Official icon: <c>shield_locked</c></summary>
    public static string? ShieldLocked => Glyph("\uf592");
    /// <summary>Official icon: <c>shield_moon</c></summary>
    public static string? ShieldMoon => Glyph("\ueaa9");
    /// <summary>Official icon: <c>shield_person</c></summary>
    public static string? ShieldPerson => Glyph("\uf650");
    /// <summary>Official icon: <c>shield_question</c></summary>
    public static string? ShieldQuestion => Glyph("\uf529");
    /// <summary>Official icon: <c>shield_radar</c></summary>
    public static string? ShieldRadar => Glyph("\U000FFF2F");
    /// <summary>Official icon: <c>shield_toggle</c></summary>
    public static string? ShieldToggle => Glyph("\uf2ad");
    /// <summary>Official icon: <c>shield_watch</c></summary>
    public static string? ShieldWatch => Glyph("\uf30f");
    /// <summary>Official icon: <c>shield_with_heart</c></summary>
    public static string? ShieldWithHeart => Glyph("\ue78f");
    /// <summary>Official icon: <c>shield_with_house</c></summary>
    public static string? ShieldWithHouse => Glyph("\ue78d");
    /// <summary>Official icon: <c>shift</c></summary>
    public static string? Shift => Glyph("\ue5f2");
    /// <summary>Official icon: <c>shift_lock</c></summary>
    public static string? ShiftLock => Glyph("\uf7ae");
    /// <summary>Official icon: <c>shift_lock_off</c></summary>
    public static string? ShiftLockOff => Glyph("\uf483");
    /// <summary>Official icon: <c>shoe_cleats</c></summary>
    public static string? ShoeCleats => Glyph("\U000FFFB3");
    /// <summary>Official icon: <c>shop</c></summary>
    public static string? Shop => Glyph("\ue8c9");
    /// <summary>Official icon: <c>shop_2</c></summary>
    public static string? Shop2 => Glyph("\ue8ca");
    /// <summary>Official icon: <c>shop_two</c></summary>
    public static string? ShopTwo => Glyph("\ue8ca");
    /// <summary>Official icon: <c>shopping_bag</c></summary>
    public static string? ShoppingBag => Glyph("\uf1cc");
    /// <summary>Official icon: <c>shopping_bag_speed</c></summary>
    public static string? ShoppingBagSpeed => Glyph("\uf39a");
    /// <summary>Official icon: <c>shopping_basket</c></summary>
    public static string? ShoppingBasket => Glyph("\ue8cb");
    /// <summary>Official icon: <c>shopping_cart</c></summary>
    public static string? ShoppingCart => Glyph("\ue8cc");
    /// <summary>Official icon: <c>shopping_cart_checkout</c></summary>
    public static string? ShoppingCartCheckout => Glyph("\ueb88");
    /// <summary>Official icon: <c>shopping_cart_off</c></summary>
    public static string? ShoppingCartOff => Glyph("\uf4f7");
    /// <summary>Official icon: <c>shoppingmode</c></summary>
    public static string? Shoppingmode => Glyph("\uefb7");
    /// <summary>Official icon: <c>short_stay</c></summary>
    public static string? ShortStay => Glyph("\ue4d0");
    /// <summary>Official icon: <c>short_text</c></summary>
    public static string? ShortText => Glyph("\ue261");
    /// <summary>Official icon: <c>shortcut</c></summary>
    public static string? Shortcut => Glyph("\uf57a");
    /// <summary>Official icon: <c>show_chart</c></summary>
    public static string? ShowChart => Glyph("\ue6e1");
    /// <summary>Official icon: <c>shower</c></summary>
    public static string? Shower => Glyph("\uf061");
    /// <summary>Official icon: <c>shuffle</c></summary>
    public static string? Shuffle => Glyph("\ue043");
    /// <summary>Official icon: <c>shuffle_on</c></summary>
    public static string? ShuffleOn => Glyph("\ue9e1");
    /// <summary>Official icon: <c>shutter_speed</c></summary>
    public static string? ShutterSpeed => Glyph("\ue43d");
    /// <summary>Official icon: <c>shutter_speed_add</c></summary>
    public static string? ShutterSpeedAdd => Glyph("\uf57e");
    /// <summary>Official icon: <c>shutter_speed_minus</c></summary>
    public static string? ShutterSpeedMinus => Glyph("\uf57d");
    /// <summary>Official icon: <c>sick</c></summary>
    public static string? Sick => Glyph("\uf220");
    /// <summary>Official icon: <c>side_navigation</c></summary>
    public static string? SideNavigation => Glyph("\ue9e2");
    /// <summary>Official icon: <c>sign_language</c></summary>
    public static string? SignLanguage => Glyph("\uebe5");
    /// <summary>Official icon: <c>sign_language_2</c></summary>
    public static string? SignLanguage2 => Glyph("\uf258");
    /// <summary>Official icon: <c>sign_language_off</c></summary>
    public static string? SignLanguageOff => Glyph("\U000FFEE4");
    /// <summary>Official icon: <c>signal_cellular_0_bar</c></summary>
    public static string? SignalCellular0Bar => Glyph("\uf0a8");
    /// <summary>Official icon: <c>signal_cellular_1_bar</c></summary>
    public static string? SignalCellular1Bar => Glyph("\uf0a9");
    /// <summary>Official icon: <c>signal_cellular_2_bar</c></summary>
    public static string? SignalCellular2Bar => Glyph("\uf0aa");
    /// <summary>Official icon: <c>signal_cellular_3_bar</c></summary>
    public static string? SignalCellular3Bar => Glyph("\uf0ab");
    /// <summary>Official icon: <c>signal_cellular_4_bar</c></summary>
    public static string? SignalCellular4Bar => Glyph("\ue1c8");
    /// <summary>Official icon: <c>signal_cellular_add</c></summary>
    public static string? SignalCellularAdd => Glyph("\uf7a9");
    /// <summary>Official icon: <c>signal_cellular_alt</c></summary>
    public static string? SignalCellularAlt => Glyph("\ue202");
    /// <summary>Official icon: <c>signal_cellular_alt_1_bar</c></summary>
    public static string? SignalCellularAlt1Bar => Glyph("\uebdf");
    /// <summary>Official icon: <c>signal_cellular_alt_2_bar</c></summary>
    public static string? SignalCellularAlt2Bar => Glyph("\uebe3");
    /// <summary>Official icon: <c>signal_cellular_alt_off</c></summary>
    public static string? SignalCellularAltOff => Glyph("\U000FFF8A");
    /// <summary>Official icon: <c>signal_cellular_connected_no_internet_0_bar</c></summary>
    public static string? SignalCellularConnectedNoInternet0Bar => Glyph("\uf0ac");
    /// <summary>Official icon: <c>signal_cellular_connected_no_internet_4_bar</c></summary>
    public static string? SignalCellularConnectedNoInternet4Bar => Glyph("\ue1cd");
    /// <summary>Official icon: <c>signal_cellular_no_sim</c></summary>
    public static string? SignalCellularNoSim => Glyph("\ue1ce");
    /// <summary>Official icon: <c>signal_cellular_nodata</c></summary>
    public static string? SignalCellularNodata => Glyph("\uf062");
    /// <summary>Official icon: <c>signal_cellular_null</c></summary>
    public static string? SignalCellularNull => Glyph("\ue1cf");
    /// <summary>Official icon: <c>signal_cellular_off</c></summary>
    public static string? SignalCellularOff => Glyph("\ue1d0");
    /// <summary>Official icon: <c>signal_cellular_pause</c></summary>
    public static string? SignalCellularPause => Glyph("\uf5a7");
    /// <summary>Official icon: <c>signal_disconnected</c></summary>
    public static string? SignalDisconnected => Glyph("\uf239");
    /// <summary>Official icon: <c>signal_wifi_0_bar</c></summary>
    public static string? SignalWifi0Bar => Glyph("\uf0b0");
    /// <summary>Official icon: <c>signal_wifi_4_bar</c></summary>
    public static string? SignalWifi4Bar => Glyph("\uf065");
    /// <summary>Official icon: <c>signal_wifi_4_bar_lock</c></summary>
    public static string? SignalWifi4BarLock => Glyph("\ue1e1");
    /// <summary>Official icon: <c>signal_wifi_bad</c></summary>
    public static string? SignalWifiBad => Glyph("\uf064");
    /// <summary>Official icon: <c>signal_wifi_connected_no_internet_4</c></summary>
    public static string? SignalWifiConnectedNoInternet4 => Glyph("\uf064");
    /// <summary>Official icon: <c>signal_wifi_off</c></summary>
    public static string? SignalWifiOff => Glyph("\ue1da");
    /// <summary>Official icon: <c>signal_wifi_statusbar_4_bar</c></summary>
    public static string? SignalWifiStatusbar4Bar => Glyph("\uf065");
    /// <summary>Official icon: <c>signal_wifi_statusbar_not_connected</c></summary>
    public static string? SignalWifiStatusbarNotConnected => Glyph("\uf0ef");
    /// <summary>Official icon: <c>signal_wifi_statusbar_null</c></summary>
    public static string? SignalWifiStatusbarNull => Glyph("\uf067");
    /// <summary>Official icon: <c>signature</c></summary>
    public static string? Signature => Glyph("\uf74c");
    /// <summary>Official icon: <c>signpost</c></summary>
    public static string? Signpost => Glyph("\ueb91");
    /// <summary>Official icon: <c>sim_card</c></summary>
    public static string? SimCard => Glyph("\ue32b");
    /// <summary>Official icon: <c>sim_card_alert</c></summary>
    public static string? SimCardAlert => Glyph("\uf057");
    /// <summary>Official icon: <c>sim_card_download</c></summary>
    public static string? SimCardDownload => Glyph("\uf068");
    /// <summary>Official icon: <c>sim_card_lock</c></summary>
    public static string? SimCardLock => Glyph("\U000FFEC1");
    /// <summary>Official icon: <c>simulation</c></summary>
    public static string? Simulation => Glyph("\uf3e1");
    /// <summary>Official icon: <c>single_arrow</c></summary>
    public static string? SingleArrow => Glyph("\U000FFED9");
    /// <summary>Official icon: <c>single_bed</c></summary>
    public static string? SingleBed => Glyph("\uea48");
    /// <summary>Official icon: <c>sip</c></summary>
    public static string? Sip => Glyph("\uf069");
    /// <summary>Official icon: <c>siren</c></summary>
    public static string? Siren => Glyph("\uf3a7");
    /// <summary>Official icon: <c>siren_check</c></summary>
    public static string? SirenCheck => Glyph("\uf3a6");
    /// <summary>Official icon: <c>siren_open</c></summary>
    public static string? SirenOpen => Glyph("\uf3a5");
    /// <summary>Official icon: <c>siren_question</c></summary>
    public static string? SirenQuestion => Glyph("\uf3a4");
    /// <summary>Official icon: <c>skateboarding</c></summary>
    public static string? Skateboarding => Glyph("\ue511");
    /// <summary>Official icon: <c>skeleton</c></summary>
    public static string? Skeleton => Glyph("\uf899");
    /// <summary>Official icon: <c>skillet</c></summary>
    public static string? Skillet => Glyph("\uf543");
    /// <summary>Official icon: <c>skillet_cooktop</c></summary>
    public static string? SkilletCooktop => Glyph("\uf544");
    /// <summary>Official icon: <c>skip_next</c></summary>
    public static string? SkipNext => Glyph("\ue044");
    /// <summary>Official icon: <c>skip_previous</c></summary>
    public static string? SkipPrevious => Glyph("\ue045");
    /// <summary>Official icon: <c>skull</c></summary>
    public static string? Skull => Glyph("\uf89a");
    /// <summary>Official icon: <c>skull_list</c></summary>
    public static string? SkullList => Glyph("\uf370");
    /// <summary>Official icon: <c>slab_serif</c></summary>
    public static string? SlabSerif => Glyph("\uf4ab");
    /// <summary>Official icon: <c>sledding</c></summary>
    public static string? Sledding => Glyph("\ue512");
    /// <summary>Official icon: <c>sleep</c></summary>
    public static string? Sleep => Glyph("\ue213");
    /// <summary>Official icon: <c>sleep_score</c></summary>
    public static string? SleepScore => Glyph("\uf6b7");
    /// <summary>Official icon: <c>slide_library</c></summary>
    public static string? SlideLibrary => Glyph("\uf822");
    /// <summary>Official icon: <c>sliders</c></summary>
    public static string? Sliders => Glyph("\ue9e3");
    /// <summary>Official icon: <c>slideshow</c></summary>
    public static string? Slideshow => Glyph("\ue41b");
    /// <summary>Official icon: <c>slow_motion_video</c></summary>
    public static string? SlowMotionVideo => Glyph("\ue068");
    /// <summary>Official icon: <c>smart_button</c></summary>
    public static string? SmartButton => Glyph("\uf1c1");
    /// <summary>Official icon: <c>smart_card_reader</c></summary>
    public static string? SmartCardReader => Glyph("\uf4a5");
    /// <summary>Official icon: <c>smart_card_reader_off</c></summary>
    public static string? SmartCardReaderOff => Glyph("\uf4a6");
    /// <summary>Official icon: <c>smart_display</c></summary>
    public static string? SmartDisplay => Glyph("\uf06a");
    /// <summary>Official icon: <c>smart_outlet</c></summary>
    public static string? SmartOutlet => Glyph("\ue844");
    /// <summary>Official icon: <c>smart_screen</c></summary>
    public static string? SmartScreen => Glyph("\uf2d0");
    /// <summary>Official icon: <c>smart_toy</c></summary>
    public static string? SmartToy => Glyph("\uf06c");
    /// <summary>Official icon: <c>smartphone</c></summary>
    public static string? Smartphone => Glyph("\ue7ba");
    /// <summary>Official icon: <c>smartphone_camera</c></summary>
    public static string? SmartphoneCamera => Glyph("\uf44e");
    /// <summary>Official icon: <c>smb_share</c></summary>
    public static string? SmbShare => Glyph("\uf74b");
    /// <summary>Official icon: <c>smoke_free</c></summary>
    public static string? SmokeFree => Glyph("\ueb4a");
    /// <summary>Official icon: <c>smoking_rooms</c></summary>
    public static string? SmokingRooms => Glyph("\ueb4b");
    /// <summary>Official icon: <c>sms</c></summary>
    public static string? Sms => Glyph("\ue625");
    /// <summary>Official icon: <c>sms_failed</c></summary>
    public static string? SmsFailed => Glyph("\ue87f");
    /// <summary>Official icon: <c>snail</c></summary>
    public static string? Snail => Glyph("\U000FFEDE");
    /// <summary>Official icon: <c>snippet_folder</c></summary>
    public static string? SnippetFolder => Glyph("\uf1c7");
    /// <summary>Official icon: <c>snooze</c></summary>
    public static string? Snooze => Glyph("\ue046");
    /// <summary>Official icon: <c>snowboarding</c></summary>
    public static string? Snowboarding => Glyph("\ue513");
    /// <summary>Official icon: <c>snowflake</c></summary>
    public static string? Snowflake => Glyph("\ued5b");
    /// <summary>Official icon: <c>snowing</c></summary>
    public static string? Snowing => Glyph("\ue80f");
    /// <summary>Official icon: <c>snowing_heavy</c></summary>
    public static string? SnowingHeavy => Glyph("\uf61c");
    /// <summary>Official icon: <c>snowmobile</c></summary>
    public static string? Snowmobile => Glyph("\ue503");
    /// <summary>Official icon: <c>snowshoeing</c></summary>
    public static string? Snowshoeing => Glyph("\ue514");
    /// <summary>Official icon: <c>soap</c></summary>
    public static string? Soap => Glyph("\uf1b2");
    /// <summary>Official icon: <c>soba</c></summary>
    public static string? Soba => Glyph("\uef36");
    /// <summary>Official icon: <c>social_distance</c></summary>
    public static string? SocialDistance => Glyph("\ue1cb");
    /// <summary>Official icon: <c>social_leaderboard</c></summary>
    public static string? SocialLeaderboard => Glyph("\uf6a0");
    /// <summary>Official icon: <c>solar_power</c></summary>
    public static string? SolarPower => Glyph("\uec0f");
    /// <summary>Official icon: <c>solo_dining</c></summary>
    public static string? SoloDining => Glyph("\uef35");
    /// <summary>Official icon: <c>sort</c></summary>
    public static string? Sort => Glyph("\ue164");
    /// <summary>Official icon: <c>sort_by_alpha</c></summary>
    public static string? SortByAlpha => Glyph("\ue053");
    /// <summary>Official icon: <c>sos</c></summary>
    public static string? Sos => Glyph("\uebf7");
    /// <summary>Official icon: <c>sound_detection_dog_barking</c></summary>
    public static string? SoundDetectionDogBarking => Glyph("\uf149");
    /// <summary>Official icon: <c>sound_detection_glass_break</c></summary>
    public static string? SoundDetectionGlassBreak => Glyph("\uf14a");
    /// <summary>Official icon: <c>sound_detection_loud_sound</c></summary>
    public static string? SoundDetectionLoudSound => Glyph("\uf14b");
    /// <summary>Official icon: <c>sound_sampler</c></summary>
    public static string? SoundSampler => Glyph("\uf6b4");
    /// <summary>Official icon: <c>soundbar</c></summary>
    public static string? Soundbar => Glyph("\U000FFF72");
    /// <summary>Official icon: <c>soup_kitchen</c></summary>
    public static string? SoupKitchen => Glyph("\ue7d3");
    /// <summary>Official icon: <c>source</c></summary>
    public static string? Source => Glyph("\uf1c8");
    /// <summary>Official icon: <c>source_environment</c></summary>
    public static string? SourceEnvironment => Glyph("\ue527");
    /// <summary>Official icon: <c>source_notes</c></summary>
    public static string? SourceNotes => Glyph("\ue12d");
    /// <summary>Official icon: <c>south</c></summary>
    public static string? South => Glyph("\uf1e3");
    /// <summary>Official icon: <c>south_america</c></summary>
    public static string? SouthAmerica => Glyph("\ue7e4");
    /// <summary>Official icon: <c>south_east</c></summary>
    public static string? SouthEast => Glyph("\uf1e4");
    /// <summary>Official icon: <c>south_west</c></summary>
    public static string? SouthWest => Glyph("\uf1e5");
    /// <summary>Official icon: <c>spa</c></summary>
    public static string? Spa => Glyph("\ueb4c");
    /// <summary>Official icon: <c>space_bar</c></summary>
    public static string? SpaceBar => Glyph("\ue256");
    /// <summary>Official icon: <c>space_dashboard</c></summary>
    public static string? SpaceDashboard => Glyph("\ue66b");
    /// <summary>Official icon: <c>space_dashboard_2</c></summary>
    public static string? SpaceDashboard2 => Glyph("\U000FFF8C");
    /// <summary>Official icon: <c>spatial_audio</c></summary>
    public static string? SpatialAudio => Glyph("\uebeb");
    /// <summary>Official icon: <c>spatial_audio_off</c></summary>
    public static string? SpatialAudioOff => Glyph("\uebe8");
    /// <summary>Official icon: <c>spatial_gallery</c></summary>
    public static string? SpatialGallery => Glyph("\U000FFEB6");
    /// <summary>Official icon: <c>spatial_speaker</c></summary>
    public static string? SpatialSpeaker => Glyph("\uf4cf");
    /// <summary>Official icon: <c>spatial_tracking</c></summary>
    public static string? SpatialTracking => Glyph("\uebea");
    /// <summary>Official icon: <c>speaker</c></summary>
    public static string? Speaker => Glyph("\ue32d");
    /// <summary>Official icon: <c>speaker_2</c></summary>
    public static string? Speaker2 => Glyph("\U000FFF71");
    /// <summary>Official icon: <c>speaker_3</c></summary>
    public static string? Speaker3 => Glyph("\U000FFEB7");
    /// <summary>Official icon: <c>speaker_group</c></summary>
    public static string? SpeakerGroup => Glyph("\ue32e");
    /// <summary>Official icon: <c>speaker_notes</c></summary>
    public static string? SpeakerNotes => Glyph("\ue8cd");
    /// <summary>Official icon: <c>speaker_notes_off</c></summary>
    public static string? SpeakerNotesOff => Glyph("\ue92a");
    /// <summary>Official icon: <c>speaker_phone</c></summary>
    public static string? SpeakerPhone => Glyph("\ue0d2");
    /// <summary>Official icon: <c>special_character</c></summary>
    public static string? SpecialCharacter => Glyph("\uf74a");
    /// <summary>Official icon: <c>specific_gravity</c></summary>
    public static string? SpecificGravity => Glyph("\uf872");
    /// <summary>Official icon: <c>speech_to_text</c></summary>
    public static string? SpeechToText => Glyph("\uf8a7");
    /// <summary>Official icon: <c>speech_to_text_2</c></summary>
    public static string? SpeechToText2 => Glyph("\U000FFEC9");
    /// <summary>Official icon: <c>speed</c></summary>
    public static string? Speed => Glyph("\ue9e4");
    /// <summary>Official icon: <c>speed_0_25</c></summary>
    public static string? Speed025 => Glyph("\uf4d4");
    /// <summary>Official icon: <c>speed_0_2x</c></summary>
    public static string? Speed02x => Glyph("\uf498");
    /// <summary>Official icon: <c>speed_0_5</c></summary>
    public static string? Speed05 => Glyph("\uf4e2");
    /// <summary>Official icon: <c>speed_0_5x</c></summary>
    public static string? Speed05x => Glyph("\uf497");
    /// <summary>Official icon: <c>speed_0_75</c></summary>
    public static string? Speed075 => Glyph("\uf4d3");
    /// <summary>Official icon: <c>speed_0_7x</c></summary>
    public static string? Speed07x => Glyph("\uf496");
    /// <summary>Official icon: <c>speed_1_2</c></summary>
    public static string? Speed12 => Glyph("\uf4e1");
    /// <summary>Official icon: <c>speed_1_25</c></summary>
    public static string? Speed125 => Glyph("\uf4d2");
    /// <summary>Official icon: <c>speed_1_2x</c></summary>
    public static string? Speed12x => Glyph("\uf495");
    /// <summary>Official icon: <c>speed_1_5</c></summary>
    public static string? Speed15 => Glyph("\uf4e0");
    /// <summary>Official icon: <c>speed_1_5x</c></summary>
    public static string? Speed15x => Glyph("\uf494");
    /// <summary>Official icon: <c>speed_1_75</c></summary>
    public static string? Speed175 => Glyph("\uf4d1");
    /// <summary>Official icon: <c>speed_1_7x</c></summary>
    public static string? Speed17x => Glyph("\uf493");
    /// <summary>Official icon: <c>speed_2</c></summary>
    public static string? Speed2 => Glyph("\U000FFF38");
    /// <summary>Official icon: <c>speed_2x</c></summary>
    public static string? Speed2x => Glyph("\uf4eb");
    /// <summary>Official icon: <c>speed_3</c></summary>
    public static string? Speed3 => Glyph("\U000FFF37");
    /// <summary>Official icon: <c>speed_4</c></summary>
    public static string? Speed4 => Glyph("\U000FFF36");
    /// <summary>Official icon: <c>speed_camera</c></summary>
    public static string? SpeedCamera => Glyph("\uf470");
    /// <summary>Official icon: <c>spellcheck</c></summary>
    public static string? Spellcheck => Glyph("\ue8ce");
    /// <summary>Official icon: <c>split_scene</c></summary>
    public static string? SplitScene => Glyph("\uf3bf");
    /// <summary>Official icon: <c>split_scene_2</c></summary>
    public static string? SplitScene2 => Glyph("\U000FFEF7");
    /// <summary>Official icon: <c>split_scene_down</c></summary>
    public static string? SplitSceneDown => Glyph("\uf2ff");
    /// <summary>Official icon: <c>split_scene_left</c></summary>
    public static string? SplitSceneLeft => Glyph("\uf2fe");
    /// <summary>Official icon: <c>split_scene_right</c></summary>
    public static string? SplitSceneRight => Glyph("\uf2fd");
    /// <summary>Official icon: <c>split_scene_up</c></summary>
    public static string? SplitSceneUp => Glyph("\uf2fc");
    /// <summary>Official icon: <c>splitscreen</c></summary>
    public static string? Splitscreen => Glyph("\uf06d");
    /// <summary>Official icon: <c>splitscreen_add</c></summary>
    public static string? SplitscreenAdd => Glyph("\uf4fd");
    /// <summary>Official icon: <c>splitscreen_bottom</c></summary>
    public static string? SplitscreenBottom => Glyph("\uf676");
    /// <summary>Official icon: <c>splitscreen_landscape</c></summary>
    public static string? SplitscreenLandscape => Glyph("\uf459");
    /// <summary>Official icon: <c>splitscreen_landscape_add</c></summary>
    public static string? SplitscreenLandscapeAdd => Glyph("\U000FFFBA");
    /// <summary>Official icon: <c>splitscreen_left</c></summary>
    public static string? SplitscreenLeft => Glyph("\uf675");
    /// <summary>Official icon: <c>splitscreen_portrait</c></summary>
    public static string? SplitscreenPortrait => Glyph("\uf458");
    /// <summary>Official icon: <c>splitscreen_right</c></summary>
    public static string? SplitscreenRight => Glyph("\uf674");
    /// <summary>Official icon: <c>splitscreen_top</c></summary>
    public static string? SplitscreenTop => Glyph("\uf673");
    /// <summary>Official icon: <c>splitscreen_vertical_add</c></summary>
    public static string? SplitscreenVerticalAdd => Glyph("\uf4fc");
    /// <summary>Official icon: <c>spo2</c></summary>
    public static string? Spo2 => Glyph("\uf6db");
    /// <summary>Official icon: <c>spoke</c></summary>
    public static string? Spoke => Glyph("\ue9a7");
    /// <summary>Official icon: <c>sports</c></summary>
    public static string? Sports => Glyph("\uea30");
    /// <summary>Official icon: <c>sports_and_outdoors</c></summary>
    public static string? SportsAndOutdoors => Glyph("\uefb8");
    /// <summary>Official icon: <c>sports_bar</c></summary>
    public static string? SportsBar => Glyph("\uf1f3");
    /// <summary>Official icon: <c>sports_baseball</c></summary>
    public static string? SportsBaseball => Glyph("\uea51");
    /// <summary>Official icon: <c>sports_basketball</c></summary>
    public static string? SportsBasketball => Glyph("\uea26");
    /// <summary>Official icon: <c>sports_cricket</c></summary>
    public static string? SportsCricket => Glyph("\uea27");
    /// <summary>Official icon: <c>sports_esports</c></summary>
    public static string? SportsEsports => Glyph("\uea28");
    /// <summary>Official icon: <c>sports_football</c></summary>
    public static string? SportsFootball => Glyph("\uea29");
    /// <summary>Official icon: <c>sports_golf</c></summary>
    public static string? SportsGolf => Glyph("\uea2a");
    /// <summary>Official icon: <c>sports_gymnastics</c></summary>
    public static string? SportsGymnastics => Glyph("\uebc4");
    /// <summary>Official icon: <c>sports_handball</c></summary>
    public static string? SportsHandball => Glyph("\uea33");
    /// <summary>Official icon: <c>sports_hockey</c></summary>
    public static string? SportsHockey => Glyph("\uea2b");
    /// <summary>Official icon: <c>sports_kabaddi</c></summary>
    public static string? SportsKabaddi => Glyph("\uea34");
    /// <summary>Official icon: <c>sports_martial_arts</c></summary>
    public static string? SportsMartialArts => Glyph("\ueae9");
    /// <summary>Official icon: <c>sports_mma</c></summary>
    public static string? SportsMma => Glyph("\uea2c");
    /// <summary>Official icon: <c>sports_motorsports</c></summary>
    public static string? SportsMotorsports => Glyph("\uea2d");
    /// <summary>Official icon: <c>sports_rugby</c></summary>
    public static string? SportsRugby => Glyph("\uea2e");
    /// <summary>Official icon: <c>sports_score</c></summary>
    public static string? SportsScore => Glyph("\uf06e");
    /// <summary>Official icon: <c>sports_soccer</c></summary>
    public static string? SportsSoccer => Glyph("\uea2f");
    /// <summary>Official icon: <c>sports_tennis</c></summary>
    public static string? SportsTennis => Glyph("\uea32");
    /// <summary>Official icon: <c>sports_volleyball</c></summary>
    public static string? SportsVolleyball => Glyph("\uea31");
    /// <summary>Official icon: <c>sprinkler</c></summary>
    public static string? Sprinkler => Glyph("\ue29a");
    /// <summary>Official icon: <c>sprint</c></summary>
    public static string? Sprint => Glyph("\uf81f");
    /// <summary>Official icon: <c>sql</c></summary>
    public static string? Sql => Glyph("\U000FFF95");
    /// <summary>Official icon: <c>square</c></summary>
    public static string? Square => Glyph("\ueb36");
    /// <summary>Official icon: <c>square_circle</c></summary>
    public static string? SquareCircle => Glyph("\ueec7");
    /// <summary>Official icon: <c>square_dot</c></summary>
    public static string? SquareDot => Glyph("\uf3b3");
    /// <summary>Official icon: <c>square_foot</c></summary>
    public static string? SquareFoot => Glyph("\uea49");
    /// <summary>Official icon: <c>ssid_chart</c></summary>
    public static string? SsidChart => Glyph("\ueb66");
    /// <summary>Official icon: <c>stack</c></summary>
    public static string? Stack => Glyph("\uf609");
    /// <summary>Official icon: <c>stack_group</c></summary>
    public static string? StackGroup => Glyph("\uf359");
    /// <summary>Official icon: <c>stack_hexagon</c></summary>
    public static string? StackHexagon => Glyph("\uf41c");
    /// <summary>Official icon: <c>stack_off</c></summary>
    public static string? StackOff => Glyph("\uf608");
    /// <summary>Official icon: <c>stack_star</c></summary>
    public static string? StackStar => Glyph("\uf607");
    /// <summary>Official icon: <c>stacked_bar_chart</c></summary>
    public static string? StackedBarChart => Glyph("\ue9e6");
    /// <summary>Official icon: <c>stacked_email</c></summary>
    public static string? StackedEmail => Glyph("\ue6c7");
    /// <summary>Official icon: <c>stacked_inbox</c></summary>
    public static string? StackedInbox => Glyph("\ue6c9");
    /// <summary>Official icon: <c>stacked_line_chart</c></summary>
    public static string? StackedLineChart => Glyph("\uf22b");
    /// <summary>Official icon: <c>stacks</c></summary>
    public static string? Stacks => Glyph("\uf500");
    /// <summary>Official icon: <c>stadia_controller</c></summary>
    public static string? StadiaController => Glyph("\uf135");
    /// <summary>Official icon: <c>stadium</c></summary>
    public static string? Stadium => Glyph("\ueb90");
    /// <summary>Official icon: <c>stairs</c></summary>
    public static string? Stairs => Glyph("\uf1a9");
    /// <summary>Official icon: <c>stairs_2</c></summary>
    public static string? Stairs2 => Glyph("\uf46c");
    /// <summary>Official icon: <c>star</c></summary>
    public static string? Star => Glyph("\uf09a");
    /// <summary>Official icon: <c>star_border</c></summary>
    public static string? StarBorder => Glyph("\uf09a");
    /// <summary>Official icon: <c>star_border_purple500</c></summary>
    public static string? StarBorderPurple500 => Glyph("\uf09a");
    /// <summary>Official icon: <c>star_half</c></summary>
    public static string? StarHalf => Glyph("\ue839");
    /// <summary>Official icon: <c>star_outline</c></summary>
    public static string? StarOutline => Glyph("\uf09a");
    /// <summary>Official icon: <c>star_purple500</c></summary>
    public static string? StarPurple500 => Glyph("\uf09a");
    /// <summary>Official icon: <c>star_rate</c></summary>
    public static string? StarRate => Glyph("\uf0ec");
    /// <summary>Official icon: <c>star_rate_half</c></summary>
    public static string? StarRateHalf => Glyph("\uec45");
    /// <summary>Official icon: <c>star_shine</c></summary>
    public static string? StarShine => Glyph("\uf31d");
    /// <summary>Official icon: <c>stars</c></summary>
    public static string? Stars => Glyph("\ue8d0");
    /// <summary>Official icon: <c>stars_2</c></summary>
    public static string? Stars2 => Glyph("\uf31c");
    /// <summary>Official icon: <c>start</c></summary>
    public static string? Start => Glyph("\ue089");
    /// <summary>Official icon: <c>stat_0</c></summary>
    public static string? Stat0 => Glyph("\ue697");
    /// <summary>Official icon: <c>stat_1</c></summary>
    public static string? Stat1 => Glyph("\ue698");
    /// <summary>Official icon: <c>stat_2</c></summary>
    public static string? Stat2 => Glyph("\ue699");
    /// <summary>Official icon: <c>stat_3</c></summary>
    public static string? Stat3 => Glyph("\ue69a");
    /// <summary>Official icon: <c>stat_minus_1</c></summary>
    public static string? StatMinus1 => Glyph("\ue69b");
    /// <summary>Official icon: <c>stat_minus_2</c></summary>
    public static string? StatMinus2 => Glyph("\ue69c");
    /// <summary>Official icon: <c>stat_minus_3</c></summary>
    public static string? StatMinus3 => Glyph("\ue69d");
    /// <summary>Official icon: <c>stay_current_landscape</c></summary>
    public static string? StayCurrentLandscape => Glyph("\ued3e");
    /// <summary>Official icon: <c>stay_current_portrait</c></summary>
    public static string? StayCurrentPortrait => Glyph("\ue7ba");
    /// <summary>Official icon: <c>stay_primary_landscape</c></summary>
    public static string? StayPrimaryLandscape => Glyph("\ued3e");
    /// <summary>Official icon: <c>stay_primary_portrait</c></summary>
    public static string? StayPrimaryPortrait => Glyph("\uf2d3");
    /// <summary>Official icon: <c>steering_wheel_cool</c></summary>
    public static string? SteeringWheelCool => Glyph("\U000FFEBD");
    /// <summary>Official icon: <c>steering_wheel_heat</c></summary>
    public static string? SteeringWheelHeat => Glyph("\uf32b");
    /// <summary>Official icon: <c>step</c></summary>
    public static string? Step => Glyph("\uf6fe");
    /// <summary>Official icon: <c>step_into</c></summary>
    public static string? StepInto => Glyph("\uf701");
    /// <summary>Official icon: <c>step_out</c></summary>
    public static string? StepOut => Glyph("\uf700");
    /// <summary>Official icon: <c>step_over</c></summary>
    public static string? StepOver => Glyph("\uf6ff");
    /// <summary>Official icon: <c>steppers</c></summary>
    public static string? Steppers => Glyph("\ue9e7");
    /// <summary>Official icon: <c>steps</c></summary>
    public static string? Steps => Glyph("\uf6da");
    /// <summary>Official icon: <c>stethoscope</c></summary>
    public static string? Stethoscope => Glyph("\uf805");
    /// <summary>Official icon: <c>stethoscope_arrow</c></summary>
    public static string? StethoscopeArrow => Glyph("\uf807");
    /// <summary>Official icon: <c>stethoscope_check</c></summary>
    public static string? StethoscopeCheck => Glyph("\uf806");
    /// <summary>Official icon: <c>sticker</c></summary>
    public static string? Sticker => Glyph("\ue707");
    /// <summary>Official icon: <c>sticker_add</c></summary>
    public static string? StickerAdd => Glyph("\ueec2");
    /// <summary>Official icon: <c>sticky_note</c></summary>
    public static string? StickyNote => Glyph("\ue9e8");
    /// <summary>Official icon: <c>sticky_note_2</c></summary>
    public static string? StickyNote2 => Glyph("\uf1fc");
    /// <summary>Official icon: <c>stock_media</c></summary>
    public static string? StockMedia => Glyph("\uf570");
    /// <summary>Official icon: <c>stockpot</c></summary>
    public static string? Stockpot => Glyph("\uf545");
    /// <summary>Official icon: <c>stop</c></summary>
    public static string? Stop => Glyph("\ue047");
    /// <summary>Official icon: <c>stop_circle</c></summary>
    public static string? StopCircle => Glyph("\uef71");
    /// <summary>Official icon: <c>stop_screen_share</c></summary>
    public static string? StopScreenShare => Glyph("\ue0e3");
    /// <summary>Official icon: <c>storage</c></summary>
    public static string? Storage => Glyph("\ue1db");
    /// <summary>Official icon: <c>store</c></summary>
    public static string? Store => Glyph("\ue8d1");
    /// <summary>Official icon: <c>store_mall_directory</c></summary>
    public static string? StoreMallDirectory => Glyph("\ue8d1");
    /// <summary>Official icon: <c>storefront</c></summary>
    public static string? Storefront => Glyph("\uea12");
    /// <summary>Official icon: <c>storm</c></summary>
    public static string? Storm => Glyph("\uf070");
    /// <summary>Official icon: <c>straight</c></summary>
    public static string? Straight => Glyph("\ueb95");
    /// <summary>Official icon: <c>straighten</c></summary>
    public static string? Straighten => Glyph("\ue41c");
    /// <summary>Official icon: <c>strategy</c></summary>
    public static string? Strategy => Glyph("\uf5df");
    /// <summary>Official icon: <c>stream</c></summary>
    public static string? Stream => Glyph("\ue9e9");
    /// <summary>Official icon: <c>stream_apps</c></summary>
    public static string? StreamApps => Glyph("\uf79f");
    /// <summary>Official icon: <c>streetview</c></summary>
    public static string? Streetview => Glyph("\ue56e");
    /// <summary>Official icon: <c>stress_management</c></summary>
    public static string? StressManagement => Glyph("\uf6d9");
    /// <summary>Official icon: <c>strikethrough_s</c></summary>
    public static string? StrikethroughS => Glyph("\ue257");
    /// <summary>Official icon: <c>stroke_full</c></summary>
    public static string? StrokeFull => Glyph("\uf749");
    /// <summary>Official icon: <c>stroke_partial</c></summary>
    public static string? StrokePartial => Glyph("\uf748");
    /// <summary>Official icon: <c>stroller</c></summary>
    public static string? Stroller => Glyph("\uf1ae");
    /// <summary>Official icon: <c>style</c></summary>
    public static string? Style => Glyph("\ue41d");
    /// <summary>Official icon: <c>styler</c></summary>
    public static string? Styler => Glyph("\ue273");
    /// <summary>Official icon: <c>stylus</c></summary>
    public static string? Stylus => Glyph("\uf604");
    /// <summary>Official icon: <c>stylus_brush</c></summary>
    public static string? StylusBrush => Glyph("\uf366");
    /// <summary>Official icon: <c>stylus_fountain_pen</c></summary>
    public static string? StylusFountainPen => Glyph("\uf365");
    /// <summary>Official icon: <c>stylus_highlighter</c></summary>
    public static string? StylusHighlighter => Glyph("\uf364");
    /// <summary>Official icon: <c>stylus_laser_pointer</c></summary>
    public static string? StylusLaserPointer => Glyph("\uf747");
    /// <summary>Official icon: <c>stylus_note</c></summary>
    public static string? StylusNote => Glyph("\uf603");
    /// <summary>Official icon: <c>stylus_pen</c></summary>
    public static string? StylusPen => Glyph("\uf363");
    /// <summary>Official icon: <c>stylus_pencil</c></summary>
    public static string? StylusPencil => Glyph("\uf362");
    /// <summary>Official icon: <c>subdirectory_arrow_left</c></summary>
    public static string? SubdirectoryArrowLeft => Glyph("\ue5d9");
    /// <summary>Official icon: <c>subdirectory_arrow_right</c></summary>
    public static string? SubdirectoryArrowRight => Glyph("\ue5da");
    /// <summary>Official icon: <c>subheader</c></summary>
    public static string? Subheader => Glyph("\ue9ea");
    /// <summary>Official icon: <c>subject</c></summary>
    public static string? Subject => Glyph("\ue8d2");
    /// <summary>Official icon: <c>subscript</c></summary>
    public static string? Subscript => Glyph("\uf111");
    /// <summary>Official icon: <c>subscriptions</c></summary>
    public static string? Subscriptions => Glyph("\ue064");
    /// <summary>Official icon: <c>subtitles</c></summary>
    public static string? Subtitles => Glyph("\ue048");
    /// <summary>Official icon: <c>subtitles_gear</c></summary>
    public static string? SubtitlesGear => Glyph("\uf355");
    /// <summary>Official icon: <c>subtitles_off</c></summary>
    public static string? SubtitlesOff => Glyph("\uef72");
    /// <summary>Official icon: <c>subway</c></summary>
    public static string? Subway => Glyph("\ue56f");
    /// <summary>Official icon: <c>subway_walk</c></summary>
    public static string? SubwayWalk => Glyph("\uf287");
    /// <summary>Official icon: <c>subwoofer</c></summary>
    public static string? Subwoofer => Glyph("\U000FFF70");
    /// <summary>Official icon: <c>summarize</c></summary>
    public static string? Summarize => Glyph("\uf071");
    /// <summary>Official icon: <c>sunny</c></summary>
    public static string? Sunny => Glyph("\ue81a");
    /// <summary>Official icon: <c>sunny_snowing</c></summary>
    public static string? SunnySnowing => Glyph("\ue819");
    /// <summary>Official icon: <c>superscript</c></summary>
    public static string? Superscript => Glyph("\uf112");
    /// <summary>Official icon: <c>supervised_user_circle</c></summary>
    public static string? SupervisedUserCircle => Glyph("\ue939");
    /// <summary>Official icon: <c>supervised_user_circle_off</c></summary>
    public static string? SupervisedUserCircleOff => Glyph("\uf60e");
    /// <summary>Official icon: <c>supervisor_account</c></summary>
    public static string? SupervisorAccount => Glyph("\ue8d3");
    /// <summary>Official icon: <c>support</c></summary>
    public static string? Support => Glyph("\uef73");
    /// <summary>Official icon: <c>support_agent</c></summary>
    public static string? SupportAgent => Glyph("\uf0e2");
    /// <summary>Official icon: <c>surfing</c></summary>
    public static string? Surfing => Glyph("\ue515");
    /// <summary>Official icon: <c>surgical</c></summary>
    public static string? Surgical => Glyph("\ue131");
    /// <summary>Official icon: <c>surround_sound</c></summary>
    public static string? SurroundSound => Glyph("\ue049");
    /// <summary>Official icon: <c>swap_calls</c></summary>
    public static string? SwapCalls => Glyph("\ue0d7");
    /// <summary>Official icon: <c>swap_driving_apps</c></summary>
    public static string? SwapDrivingApps => Glyph("\ue69e");
    /// <summary>Official icon: <c>swap_driving_apps_wheel</c></summary>
    public static string? SwapDrivingAppsWheel => Glyph("\ue69f");
    /// <summary>Official icon: <c>swap_horiz</c></summary>
    public static string? SwapHoriz => Glyph("\ue8d4");
    /// <summary>Official icon: <c>swap_horizontal_circle</c></summary>
    public static string? SwapHorizontalCircle => Glyph("\ue933");
    /// <summary>Official icon: <c>swap_vert</c></summary>
    public static string? SwapVert => Glyph("\ue8d5");
    /// <summary>Official icon: <c>swap_vertical_circle</c></summary>
    public static string? SwapVerticalCircle => Glyph("\ue8d6");
    /// <summary>Official icon: <c>sweep</c></summary>
    public static string? Sweep => Glyph("\ue6ac");
    /// <summary>Official icon: <c>swipe</c></summary>
    public static string? Swipe => Glyph("\ue9ec");
    /// <summary>Official icon: <c>swipe_down</c></summary>
    public static string? SwipeDown => Glyph("\ueb53");
    /// <summary>Official icon: <c>swipe_down_alt</c></summary>
    public static string? SwipeDownAlt => Glyph("\ueb30");
    /// <summary>Official icon: <c>swipe_left</c></summary>
    public static string? SwipeLeft => Glyph("\ueb59");
    /// <summary>Official icon: <c>swipe_left_2</c></summary>
    public static string? SwipeLeft2 => Glyph("\U000FFF94");
    /// <summary>Official icon: <c>swipe_left_alt</c></summary>
    public static string? SwipeLeftAlt => Glyph("\ueb33");
    /// <summary>Official icon: <c>swipe_right</c></summary>
    public static string? SwipeRight => Glyph("\ueb52");
    /// <summary>Official icon: <c>swipe_right_2</c></summary>
    public static string? SwipeRight2 => Glyph("\U000FFF93");
    /// <summary>Official icon: <c>swipe_right_alt</c></summary>
    public static string? SwipeRightAlt => Glyph("\ueb56");
    /// <summary>Official icon: <c>swipe_up</c></summary>
    public static string? SwipeUp => Glyph("\ueb2e");
    /// <summary>Official icon: <c>swipe_up_alt</c></summary>
    public static string? SwipeUpAlt => Glyph("\ueb35");
    /// <summary>Official icon: <c>swipe_vertical</c></summary>
    public static string? SwipeVertical => Glyph("\ueb51");
    /// <summary>Official icon: <c>switch</c></summary>
    public static string? Switch => Glyph("\ue1f4");
    /// <summary>Official icon: <c>switch_access</c></summary>
    public static string? SwitchAccess => Glyph("\uf6fd");
    /// <summary>Official icon: <c>switch_access_2</c></summary>
    public static string? SwitchAccess2 => Glyph("\uf506");
    /// <summary>Official icon: <c>switch_access_3</c></summary>
    public static string? SwitchAccess3 => Glyph("\uf34d");
    /// <summary>Official icon: <c>switch_access_shortcut</c></summary>
    public static string? SwitchAccessShortcut => Glyph("\ue7e1");
    /// <summary>Official icon: <c>switch_access_shortcut_add</c></summary>
    public static string? SwitchAccessShortcutAdd => Glyph("\ue7e2");
    /// <summary>Official icon: <c>switch_account</c></summary>
    public static string? SwitchAccount => Glyph("\ue9ed");
    /// <summary>Official icon: <c>switch_camera</c></summary>
    public static string? SwitchCamera => Glyph("\ue41e");
    /// <summary>Official icon: <c>switch_left</c></summary>
    public static string? SwitchLeft => Glyph("\uf1d1");
    /// <summary>Official icon: <c>switch_off</c></summary>
    public static string? SwitchOff => Glyph("\U000FFF6F");
    /// <summary>Official icon: <c>switch_right</c></summary>
    public static string? SwitchRight => Glyph("\uf1d2");
    /// <summary>Official icon: <c>switch_video</c></summary>
    public static string? SwitchVideo => Glyph("\ue41f");
    /// <summary>Official icon: <c>switches</c></summary>
    public static string? Switches => Glyph("\ue733");
    /// <summary>Official icon: <c>sword_rose</c></summary>
    public static string? SwordRose => Glyph("\uf5de");
    /// <summary>Official icon: <c>swords</c></summary>
    public static string? Swords => Glyph("\uf889");
    /// <summary>Official icon: <c>symptoms</c></summary>
    public static string? Symptoms => Glyph("\ue132");
    /// <summary>Official icon: <c>synagogue</c></summary>
    public static string? Synagogue => Glyph("\ueab0");
    /// <summary>Official icon: <c>sync</c></summary>
    public static string? Sync => Glyph("\ue627");
    /// <summary>Official icon: <c>sync_alt</c></summary>
    public static string? SyncAlt => Glyph("\uea18");
    /// <summary>Official icon: <c>sync_arrow_down</c></summary>
    public static string? SyncArrowDown => Glyph("\uf37c");
    /// <summary>Official icon: <c>sync_arrow_up</c></summary>
    public static string? SyncArrowUp => Glyph("\uf37b");
    /// <summary>Official icon: <c>sync_desktop</c></summary>
    public static string? SyncDesktop => Glyph("\uf41a");
    /// <summary>Official icon: <c>sync_disabled</c></summary>
    public static string? SyncDisabled => Glyph("\ue628");
    /// <summary>Official icon: <c>sync_lock</c></summary>
    public static string? SyncLock => Glyph("\ueaee");
    /// <summary>Official icon: <c>sync_problem</c></summary>
    public static string? SyncProblem => Glyph("\ue629");
    /// <summary>Official icon: <c>sync_saved_locally</c></summary>
    public static string? SyncSavedLocally => Glyph("\uf820");
    /// <summary>Official icon: <c>sync_saved_locally_off</c></summary>
    public static string? SyncSavedLocallyOff => Glyph("\uf264");
    /// <summary>Official icon: <c>syringe</c></summary>
    public static string? Syringe => Glyph("\ue133");
    /// <summary>Official icon: <c>system_security_update</c></summary>
    public static string? SystemSecurityUpdate => Glyph("\uf2cd");
    /// <summary>Official icon: <c>system_security_update_good</c></summary>
    public static string? SystemSecurityUpdateGood => Glyph("\uf073");
    /// <summary>Official icon: <c>system_security_update_warning</c></summary>
    public static string? SystemSecurityUpdateWarning => Glyph("\uf2d3");
    /// <summary>Official icon: <c>system_update</c></summary>
    public static string? SystemUpdate => Glyph("\uf2cd");
    /// <summary>Official icon: <c>system_update_alt</c></summary>
    public static string? SystemUpdateAlt => Glyph("\ue8d7");
    /// <summary>Official icon: <c>tab</c></summary>
    public static string? Tab => Glyph("\ue8d8");
    /// <summary>Official icon: <c>tab_close</c></summary>
    public static string? TabClose => Glyph("\uf745");
    /// <summary>Official icon: <c>tab_close_inactive</c></summary>
    public static string? TabCloseInactive => Glyph("\uf3d0");
    /// <summary>Official icon: <c>tab_close_right</c></summary>
    public static string? TabCloseRight => Glyph("\uf746");
    /// <summary>Official icon: <c>tab_duplicate</c></summary>
    public static string? TabDuplicate => Glyph("\uf744");
    /// <summary>Official icon: <c>tab_group</c></summary>
    public static string? TabGroup => Glyph("\uf743");
    /// <summary>Official icon: <c>tab_inactive</c></summary>
    public static string? TabInactive => Glyph("\uf43b");
    /// <summary>Official icon: <c>tab_move</c></summary>
    public static string? TabMove => Glyph("\uf742");
    /// <summary>Official icon: <c>tab_new_right</c></summary>
    public static string? TabNewRight => Glyph("\uf741");
    /// <summary>Official icon: <c>tab_recent</c></summary>
    public static string? TabRecent => Glyph("\uf740");
    /// <summary>Official icon: <c>tab_search</c></summary>
    public static string? TabSearch => Glyph("\uf2f2");
    /// <summary>Official icon: <c>tab_unselected</c></summary>
    public static string? TabUnselected => Glyph("\ue8d9");
    /// <summary>Official icon: <c>table</c></summary>
    public static string? Table => Glyph("\uf191");
    /// <summary>Official icon: <c>table_bar</c></summary>
    public static string? TableBar => Glyph("\uead2");
    /// <summary>Official icon: <c>table_chart</c></summary>
    public static string? TableChart => Glyph("\ue265");
    /// <summary>Official icon: <c>table_chart_view</c></summary>
    public static string? TableChartView => Glyph("\uf6ef");
    /// <summary>Official icon: <c>table_convert</c></summary>
    public static string? TableConvert => Glyph("\uf3c7");
    /// <summary>Official icon: <c>table_edit</c></summary>
    public static string? TableEdit => Glyph("\uf3c6");
    /// <summary>Official icon: <c>table_eye</c></summary>
    public static string? TableEye => Glyph("\uf466");
    /// <summary>Official icon: <c>table_lamp</c></summary>
    public static string? TableLamp => Glyph("\ue1f2");
    /// <summary>Official icon: <c>table_large</c></summary>
    public static string? TableLarge => Glyph("\uf299");
    /// <summary>Official icon: <c>table_restaurant</c></summary>
    public static string? TableRestaurant => Glyph("\ueac6");
    /// <summary>Official icon: <c>table_rows</c></summary>
    public static string? TableRows => Glyph("\uf101");
    /// <summary>Official icon: <c>table_rows_narrow</c></summary>
    public static string? TableRowsNarrow => Glyph("\uf73f");
    /// <summary>Official icon: <c>table_sign</c></summary>
    public static string? TableSign => Glyph("\uef2c");
    /// <summary>Official icon: <c>table_view</c></summary>
    public static string? TableView => Glyph("\uf1be");
    /// <summary>Official icon: <c>tablet</c></summary>
    public static string? Tablet => Glyph("\ue32f");
    /// <summary>Official icon: <c>tablet_android</c></summary>
    public static string? TabletAndroid => Glyph("\ue330");
    /// <summary>Official icon: <c>tablet_camera</c></summary>
    public static string? TabletCamera => Glyph("\uf44d");
    /// <summary>Official icon: <c>tablet_mac</c></summary>
    public static string? TabletMac => Glyph("\ue331");
    /// <summary>Official icon: <c>tabs</c></summary>
    public static string? Tabs => Glyph("\ue9ee");
    /// <summary>Official icon: <c>tactic</c></summary>
    public static string? Tactic => Glyph("\uf564");
    /// <summary>Official icon: <c>tag</c></summary>
    public static string? Tag => Glyph("\ue9ef");
    /// <summary>Official icon: <c>tag_faces</c></summary>
    public static string? TagFaces => Glyph("\uea22");
    /// <summary>Official icon: <c>takeout_dining</c></summary>
    public static string? TakeoutDining => Glyph("\uea74");
    /// <summary>Official icon: <c>takeout_dining_2</c></summary>
    public static string? TakeoutDining2 => Glyph("\uef34");
    /// <summary>Official icon: <c>tamper_detection_off</c></summary>
    public static string? TamperDetectionOff => Glyph("\ue82e");
    /// <summary>Official icon: <c>tamper_detection_on</c></summary>
    public static string? TamperDetectionOn => Glyph("\uf8c8");
    /// <summary>Official icon: <c>tap_and_play</c></summary>
    public static string? TapAndPlay => Glyph("\uf2cc");
    /// <summary>Official icon: <c>tapas</c></summary>
    public static string? Tapas => Glyph("\uf1e9");
    /// <summary>Official icon: <c>target</c></summary>
    public static string? Target => Glyph("\ue719");
    /// <summary>Official icon: <c>target_check</c></summary>
    public static string? TargetCheck => Glyph("\U000FFEB3");
    /// <summary>Official icon: <c>task</c></summary>
    public static string? Task => Glyph("\uf075");
    /// <summary>Official icon: <c>task_alt</c></summary>
    public static string? TaskAlt => Glyph("\ue2e6");
    /// <summary>Official icon: <c>tatami_seat</c></summary>
    public static string? TatamiSeat => Glyph("\uef33");
    /// <summary>Official icon: <c>taunt</c></summary>
    public static string? Taunt => Glyph("\uf69f");
    /// <summary>Official icon: <c>taxi_alert</c></summary>
    public static string? TaxiAlert => Glyph("\uef74");
    /// <summary>Official icon: <c>team_dashboard</c></summary>
    public static string? TeamDashboard => Glyph("\ue013");
    /// <summary>Official icon: <c>temp_preferences_custom</c></summary>
    public static string? TempPreferencesCustom => Glyph("\uf8c9");
    /// <summary>Official icon: <c>temp_preferences_eco</c></summary>
    public static string? TempPreferencesEco => Glyph("\uf8ca");
    /// <summary>Official icon: <c>temple_buddhist</c></summary>
    public static string? TempleBuddhist => Glyph("\ueab3");
    /// <summary>Official icon: <c>temple_hindu</c></summary>
    public static string? TempleHindu => Glyph("\ueaaf");
    /// <summary>Official icon: <c>tenancy</c></summary>
    public static string? Tenancy => Glyph("\uf0e3");
    /// <summary>Official icon: <c>terminal</c></summary>
    public static string? Terminal => Glyph("\ueb8e");
    /// <summary>Official icon: <c>terminal_2</c></summary>
    public static string? Terminal2 => Glyph("\U000FFF8E");
    /// <summary>Official icon: <c>terminal_add</c></summary>
    public static string? TerminalAdd => Glyph("\U000FFED3");
    /// <summary>Official icon: <c>terrain</c></summary>
    public static string? Terrain => Glyph("\ue564");
    /// <summary>Official icon: <c>text_ad</c></summary>
    public static string? TextAd => Glyph("\ue728");
    /// <summary>Official icon: <c>text_ad_off</c></summary>
    public static string? TextAdOff => Glyph("\U000FFF92");
    /// <summary>Official icon: <c>text_compare</c></summary>
    public static string? TextCompare => Glyph("\uf3c5");
    /// <summary>Official icon: <c>text_decrease</c></summary>
    public static string? TextDecrease => Glyph("\ueadd");
    /// <summary>Official icon: <c>text_fields</c></summary>
    public static string? TextFields => Glyph("\ue262");
    /// <summary>Official icon: <c>text_fields_alt</c></summary>
    public static string? TextFieldsAlt => Glyph("\ue9f1");
    /// <summary>Official icon: <c>text_format</c></summary>
    public static string? TextFormat => Glyph("\ue165");
    /// <summary>Official icon: <c>text_increase</c></summary>
    public static string? TextIncrease => Glyph("\ueae2");
    /// <summary>Official icon: <c>text_rotate_up</c></summary>
    public static string? TextRotateUp => Glyph("\ue93a");
    /// <summary>Official icon: <c>text_rotate_vertical</c></summary>
    public static string? TextRotateVertical => Glyph("\ue93b");
    /// <summary>Official icon: <c>text_rotation_angledown</c></summary>
    public static string? TextRotationAngledown => Glyph("\ue93c");
    /// <summary>Official icon: <c>text_rotation_angleup</c></summary>
    public static string? TextRotationAngleup => Glyph("\ue93d");
    /// <summary>Official icon: <c>text_rotation_down</c></summary>
    public static string? TextRotationDown => Glyph("\ue93e");
    /// <summary>Official icon: <c>text_rotation_none</c></summary>
    public static string? TextRotationNone => Glyph("\ue93f");
    /// <summary>Official icon: <c>text_select_end</c></summary>
    public static string? TextSelectEnd => Glyph("\uf73e");
    /// <summary>Official icon: <c>text_select_jump_to_beginning</c></summary>
    public static string? TextSelectJumpToBeginning => Glyph("\uf73d");
    /// <summary>Official icon: <c>text_select_jump_to_end</c></summary>
    public static string? TextSelectJumpToEnd => Glyph("\uf73c");
    /// <summary>Official icon: <c>text_select_move_back_character</c></summary>
    public static string? TextSelectMoveBackCharacter => Glyph("\uf73b");
    /// <summary>Official icon: <c>text_select_move_back_word</c></summary>
    public static string? TextSelectMoveBackWord => Glyph("\uf73a");
    /// <summary>Official icon: <c>text_select_move_down</c></summary>
    public static string? TextSelectMoveDown => Glyph("\uf739");
    /// <summary>Official icon: <c>text_select_move_forward_character</c></summary>
    public static string? TextSelectMoveForwardCharacter => Glyph("\uf738");
    /// <summary>Official icon: <c>text_select_move_forward_word</c></summary>
    public static string? TextSelectMoveForwardWord => Glyph("\uf737");
    /// <summary>Official icon: <c>text_select_move_up</c></summary>
    public static string? TextSelectMoveUp => Glyph("\uf736");
    /// <summary>Official icon: <c>text_select_start</c></summary>
    public static string? TextSelectStart => Glyph("\uf735");
    /// <summary>Official icon: <c>text_snippet</c></summary>
    public static string? TextSnippet => Glyph("\uf1c6");
    /// <summary>Official icon: <c>text_to_speech</c></summary>
    public static string? TextToSpeech => Glyph("\uf1bc");
    /// <summary>Official icon: <c>text_up</c></summary>
    public static string? TextUp => Glyph("\uf49e");
    /// <summary>Official icon: <c>textsms</c></summary>
    public static string? Textsms => Glyph("\ue625");
    /// <summary>Official icon: <c>texture</c></summary>
    public static string? Texture => Glyph("\ue421");
    /// <summary>Official icon: <c>texture_add</c></summary>
    public static string? TextureAdd => Glyph("\uf57c");
    /// <summary>Official icon: <c>texture_minus</c></summary>
    public static string? TextureMinus => Glyph("\uf57b");
    /// <summary>Official icon: <c>theater_comedy</c></summary>
    public static string? TheaterComedy => Glyph("\uea66");
    /// <summary>Official icon: <c>theaters</c></summary>
    public static string? Theaters => Glyph("\ue8da");
    /// <summary>Official icon: <c>thermometer</c></summary>
    public static string? Thermometer => Glyph("\ue846");
    /// <summary>Official icon: <c>thermometer_add</c></summary>
    public static string? ThermometerAdd => Glyph("\uf582");
    /// <summary>Official icon: <c>thermometer_alert</c></summary>
    public static string? ThermometerAlert => Glyph("\U000FFFFB");
    /// <summary>Official icon: <c>thermometer_gain</c></summary>
    public static string? ThermometerGain => Glyph("\uf6d8");
    /// <summary>Official icon: <c>thermometer_loss</c></summary>
    public static string? ThermometerLoss => Glyph("\uf6d7");
    /// <summary>Official icon: <c>thermometer_minus</c></summary>
    public static string? ThermometerMinus => Glyph("\uf581");
    /// <summary>Official icon: <c>thermostat</c></summary>
    public static string? Thermostat => Glyph("\uf076");
    /// <summary>Official icon: <c>thermostat_arrow_down</c></summary>
    public static string? ThermostatArrowDown => Glyph("\uf37a");
    /// <summary>Official icon: <c>thermostat_arrow_up</c></summary>
    public static string? ThermostatArrowUp => Glyph("\uf379");
    /// <summary>Official icon: <c>thermostat_auto</c></summary>
    public static string? ThermostatAuto => Glyph("\uf077");
    /// <summary>Official icon: <c>thermostat_carbon</c></summary>
    public static string? ThermostatCarbon => Glyph("\uf178");
    /// <summary>Official icon: <c>things_to_do</c></summary>
    public static string? ThingsToDo => Glyph("\ueb2a");
    /// <summary>Official icon: <c>thread_unread</c></summary>
    public static string? ThreadUnread => Glyph("\uf4f9");
    /// <summary>Official icon: <c>threat_intelligence</c></summary>
    public static string? ThreatIntelligence => Glyph("\ueaed");
    /// <summary>Official icon: <c>thumb_down</c></summary>
    public static string? ThumbDown => Glyph("\uf578");
    /// <summary>Official icon: <c>thumb_down_alt</c></summary>
    public static string? ThumbDownAlt => Glyph("\uf578");
    /// <summary>Official icon: <c>thumb_down_filled</c></summary>
    public static string? ThumbDownFilled => Glyph("\uf578");
    /// <summary>Official icon: <c>thumb_down_off</c></summary>
    public static string? ThumbDownOff => Glyph("\uf578");
    /// <summary>Official icon: <c>thumb_down_off_alt</c></summary>
    public static string? ThumbDownOffAlt => Glyph("\uf578");
    /// <summary>Official icon: <c>thumb_up</c></summary>
    public static string? ThumbUp => Glyph("\uf577");
    /// <summary>Official icon: <c>thumb_up_alt</c></summary>
    public static string? ThumbUpAlt => Glyph("\uf577");
    /// <summary>Official icon: <c>thumb_up_filled</c></summary>
    public static string? ThumbUpFilled => Glyph("\uf577");
    /// <summary>Official icon: <c>thumb_up_off</c></summary>
    public static string? ThumbUpOff => Glyph("\uf577");
    /// <summary>Official icon: <c>thumb_up_off_alt</c></summary>
    public static string? ThumbUpOffAlt => Glyph("\uf577");
    /// <summary>Official icon: <c>thumbnail_bar</c></summary>
    public static string? ThumbnailBar => Glyph("\uf734");
    /// <summary>Official icon: <c>thumbs_up_double</c></summary>
    public static string? ThumbsUpDouble => Glyph("\ueefc");
    /// <summary>Official icon: <c>thumbs_up_down</c></summary>
    public static string? ThumbsUpDown => Glyph("\ue8dd");
    /// <summary>Official icon: <c>thunderstorm</c></summary>
    public static string? Thunderstorm => Glyph("\uebdb");
    /// <summary>Official icon: <c>tibia</c></summary>
    public static string? Tibia => Glyph("\uf89b");
    /// <summary>Official icon: <c>tibia_alt</c></summary>
    public static string? TibiaAlt => Glyph("\uf89c");
    /// <summary>Official icon: <c>tile_large</c></summary>
    public static string? TileLarge => Glyph("\uf3c3");
    /// <summary>Official icon: <c>tile_medium</c></summary>
    public static string? TileMedium => Glyph("\uf3c2");
    /// <summary>Official icon: <c>tile_small</c></summary>
    public static string? TileSmall => Glyph("\uf3c1");
    /// <summary>Official icon: <c>tilt_arrow_down</c></summary>
    public static string? TiltArrowDown => Glyph("\U000FFF26");
    /// <summary>Official icon: <c>tilt_arrow_up</c></summary>
    public static string? TiltArrowUp => Glyph("\U000FFF25");
    /// <summary>Official icon: <c>time_auto</c></summary>
    public static string? TimeAuto => Glyph("\uf0e4");
    /// <summary>Official icon: <c>time_to_leave</c></summary>
    public static string? TimeToLeave => Glyph("\ueff7");
    /// <summary>Official icon: <c>timelapse</c></summary>
    public static string? Timelapse => Glyph("\ue422");
    /// <summary>Official icon: <c>timeline</c></summary>
    public static string? Timeline => Glyph("\ue922");
    /// <summary>Official icon: <c>timer</c></summary>
    public static string? Timer => Glyph("\ue425");
    /// <summary>Official icon: <c>timer_1</c></summary>
    public static string? Timer1 => Glyph("\uf2af");
    /// <summary>Official icon: <c>timer_10</c></summary>
    public static string? Timer10 => Glyph("\ue423");
    /// <summary>Official icon: <c>timer_10_alt_1</c></summary>
    public static string? Timer10Alt1 => Glyph("\uefbf");
    /// <summary>Official icon: <c>timer_10_select</c></summary>
    public static string? Timer10Select => Glyph("\uf07a");
    /// <summary>Official icon: <c>timer_2</c></summary>
    public static string? Timer2 => Glyph("\uf2ae");
    /// <summary>Official icon: <c>timer_3</c></summary>
    public static string? Timer3 => Glyph("\ue424");
    /// <summary>Official icon: <c>timer_3_alt_1</c></summary>
    public static string? Timer3Alt1 => Glyph("\uefc0");
    /// <summary>Official icon: <c>timer_3_select</c></summary>
    public static string? Timer3Select => Glyph("\uf07b");
    /// <summary>Official icon: <c>timer_5</c></summary>
    public static string? Timer5 => Glyph("\uf4b1");
    /// <summary>Official icon: <c>timer_5_shutter</c></summary>
    public static string? Timer5Shutter => Glyph("\uf4b2");
    /// <summary>Official icon: <c>timer_arrow_down</c></summary>
    public static string? TimerArrowDown => Glyph("\uf378");
    /// <summary>Official icon: <c>timer_arrow_up</c></summary>
    public static string? TimerArrowUp => Glyph("\uf377");
    /// <summary>Official icon: <c>timer_off</c></summary>
    public static string? TimerOff => Glyph("\ue426");
    /// <summary>Official icon: <c>timer_pause</c></summary>
    public static string? TimerPause => Glyph("\uf4bb");
    /// <summary>Official icon: <c>timer_play</c></summary>
    public static string? TimerPlay => Glyph("\uf4ba");
    /// <summary>Official icon: <c>tips_and_updates</c></summary>
    public static string? TipsAndUpdates => Glyph("\ue79a");
    /// <summary>Official icon: <c>tire_repair</c></summary>
    public static string? TireRepair => Glyph("\uebc8");
    /// <summary>Official icon: <c>title</c></summary>
    public static string? Title => Glyph("\ue264");
    /// <summary>Official icon: <c>titlecase</c></summary>
    public static string? Titlecase => Glyph("\uf489");
    /// <summary>Official icon: <c>toast</c></summary>
    public static string? Toast => Glyph("\uefc1");
    /// <summary>Official icon: <c>toc</c></summary>
    public static string? Toc => Glyph("\ue8de");
    /// <summary>Official icon: <c>today</c></summary>
    public static string? Today => Glyph("\ue8df");
    /// <summary>Official icon: <c>toggle_off</c></summary>
    public static string? ToggleOff => Glyph("\ue9f5");
    /// <summary>Official icon: <c>toggle_on</c></summary>
    public static string? ToggleOn => Glyph("\ue9f6");
    /// <summary>Official icon: <c>token</c></summary>
    public static string? Token => Glyph("\uea25");
    /// <summary>Official icon: <c>toll</c></summary>
    public static string? Toll => Glyph("\ue8e0");
    /// <summary>Official icon: <c>tonality</c></summary>
    public static string? Tonality => Glyph("\ue427");
    /// <summary>Official icon: <c>tonality_2</c></summary>
    public static string? Tonality2 => Glyph("\uf2b4");
    /// <summary>Official icon: <c>toolbar</c></summary>
    public static string? Toolbar => Glyph("\ue9f7");
    /// <summary>Official icon: <c>tools_flat_head</c></summary>
    public static string? ToolsFlatHead => Glyph("\uf8cb");
    /// <summary>Official icon: <c>tools_installation_kit</c></summary>
    public static string? ToolsInstallationKit => Glyph("\ue2ab");
    /// <summary>Official icon: <c>tools_ladder</c></summary>
    public static string? ToolsLadder => Glyph("\ue2cb");
    /// <summary>Official icon: <c>tools_level</c></summary>
    public static string? ToolsLevel => Glyph("\ue77b");
    /// <summary>Official icon: <c>tools_phillips</c></summary>
    public static string? ToolsPhillips => Glyph("\uf8cc");
    /// <summary>Official icon: <c>tools_pliers_wire_stripper</c></summary>
    public static string? ToolsPliersWireStripper => Glyph("\ue2aa");
    /// <summary>Official icon: <c>tools_power_drill</c></summary>
    public static string? ToolsPowerDrill => Glyph("\ue1e9");
    /// <summary>Official icon: <c>tools_wrench</c></summary>
    public static string? ToolsWrench => Glyph("\uf8cd");
    /// <summary>Official icon: <c>tooltip</c></summary>
    public static string? Tooltip => Glyph("\ue9f8");
    /// <summary>Official icon: <c>tooltip_2</c></summary>
    public static string? Tooltip2 => Glyph("\uf3ed");
    /// <summary>Official icon: <c>top_panel_close</c></summary>
    public static string? TopPanelClose => Glyph("\uf733");
    /// <summary>Official icon: <c>top_panel_open</c></summary>
    public static string? TopPanelOpen => Glyph("\uf732");
    /// <summary>Official icon: <c>topic</c></summary>
    public static string? Topic => Glyph("\uf1c8");
    /// <summary>Official icon: <c>tornado</c></summary>
    public static string? Tornado => Glyph("\ue199");
    /// <summary>Official icon: <c>total_dissolved_solids</c></summary>
    public static string? TotalDissolvedSolids => Glyph("\uf877");
    /// <summary>Official icon: <c>touch_app</c></summary>
    public static string? TouchApp => Glyph("\ue913");
    /// <summary>Official icon: <c>touch_double</c></summary>
    public static string? TouchDouble => Glyph("\uf38b");
    /// <summary>Official icon: <c>touch_double_2</c></summary>
    public static string? TouchDouble2 => Glyph("\U000FFF35");
    /// <summary>Official icon: <c>touch_long</c></summary>
    public static string? TouchLong => Glyph("\uf38a");
    /// <summary>Official icon: <c>touch_triple</c></summary>
    public static string? TouchTriple => Glyph("\uf389");
    /// <summary>Official icon: <c>touchpad_mouse</c></summary>
    public static string? TouchpadMouse => Glyph("\uf687");
    /// <summary>Official icon: <c>touchpad_mouse_off</c></summary>
    public static string? TouchpadMouseOff => Glyph("\uf4e6");
    /// <summary>Official icon: <c>tour</c></summary>
    public static string? Tour => Glyph("\uef75");
    /// <summary>Official icon: <c>toys</c></summary>
    public static string? Toys => Glyph("\ue332");
    /// <summary>Official icon: <c>toys_and_games</c></summary>
    public static string? ToysAndGames => Glyph("\uefc2");
    /// <summary>Official icon: <c>toys_fan</c></summary>
    public static string? ToysFan => Glyph("\uf887");
    /// <summary>Official icon: <c>track_changes</c></summary>
    public static string? TrackChanges => Glyph("\ue8e1");
    /// <summary>Official icon: <c>trackpad_input</c></summary>
    public static string? TrackpadInput => Glyph("\uf4c7");
    /// <summary>Official icon: <c>trackpad_input_2</c></summary>
    public static string? TrackpadInput2 => Glyph("\uf409");
    /// <summary>Official icon: <c>trackpad_input_3</c></summary>
    public static string? TrackpadInput3 => Glyph("\uf408");
    /// <summary>Official icon: <c>traffic</c></summary>
    public static string? Traffic => Glyph("\ue565");
    /// <summary>Official icon: <c>traffic_jam</c></summary>
    public static string? TrafficJam => Glyph("\uf46f");
    /// <summary>Official icon: <c>trail_length</c></summary>
    public static string? TrailLength => Glyph("\ueb5e");
    /// <summary>Official icon: <c>trail_length_medium</c></summary>
    public static string? TrailLengthMedium => Glyph("\ueb63");
    /// <summary>Official icon: <c>trail_length_short</c></summary>
    public static string? TrailLengthShort => Glyph("\ueb6d");
    /// <summary>Official icon: <c>train</c></summary>
    public static string? Train => Glyph("\ue570");
    /// <summary>Official icon: <c>tram</c></summary>
    public static string? Tram => Glyph("\ue571");
    /// <summary>Official icon: <c>transcribe</c></summary>
    public static string? Transcribe => Glyph("\uf8ec");
    /// <summary>Official icon: <c>transfer_within_a_station</c></summary>
    public static string? TransferWithinAStation => Glyph("\ue572");
    /// <summary>Official icon: <c>transform</c></summary>
    public static string? Transform => Glyph("\ue428");
    /// <summary>Official icon: <c>transgender</c></summary>
    public static string? Transgender => Glyph("\ue58d");
    /// <summary>Official icon: <c>transit_enterexit</c></summary>
    public static string? TransitEnterexit => Glyph("\ue579");
    /// <summary>Official icon: <c>transit_ticket</c></summary>
    public static string? TransitTicket => Glyph("\uf3f1");
    /// <summary>Official icon: <c>transition_chop</c></summary>
    public static string? TransitionChop => Glyph("\uf50e");
    /// <summary>Official icon: <c>transition_dissolve</c></summary>
    public static string? TransitionDissolve => Glyph("\uf50d");
    /// <summary>Official icon: <c>transition_fade</c></summary>
    public static string? TransitionFade => Glyph("\uf50c");
    /// <summary>Official icon: <c>transition_push</c></summary>
    public static string? TransitionPush => Glyph("\uf50b");
    /// <summary>Official icon: <c>transition_slide</c></summary>
    public static string? TransitionSlide => Glyph("\uf50a");
    /// <summary>Official icon: <c>translate</c></summary>
    public static string? Translate => Glyph("\ue8e2");
    /// <summary>Official icon: <c>translate_indic</c></summary>
    public static string? TranslateIndic => Glyph("\uf263");
    /// <summary>Official icon: <c>transportation</c></summary>
    public static string? Transportation => Glyph("\ue21d");
    /// <summary>Official icon: <c>travel</c></summary>
    public static string? Travel => Glyph("\uef93");
    /// <summary>Official icon: <c>travel_explore</c></summary>
    public static string? TravelExplore => Glyph("\ue2db");
    /// <summary>Official icon: <c>travel_luggage_and_bags</c></summary>
    public static string? TravelLuggageAndBags => Glyph("\uefc3");
    /// <summary>Official icon: <c>trending_down</c></summary>
    public static string? TrendingDown => Glyph("\ue8e3");
    /// <summary>Official icon: <c>trending_flat</c></summary>
    public static string? TrendingFlat => Glyph("\ue8e4");
    /// <summary>Official icon: <c>trending_up</c></summary>
    public static string? TrendingUp => Glyph("\ue8e5");
    /// <summary>Official icon: <c>triangle_circle</c></summary>
    public static string? TriangleCircle => Glyph("\ueec6");
    /// <summary>Official icon: <c>trip</c></summary>
    public static string? Trip => Glyph("\ue6fb");
    /// <summary>Official icon: <c>trip_origin</c></summary>
    public static string? TripOrigin => Glyph("\ue57b");
    /// <summary>Official icon: <c>trolley</c></summary>
    public static string? Trolley => Glyph("\uf86b");
    /// <summary>Official icon: <c>trolley_cable_car</c></summary>
    public static string? TrolleyCableCar => Glyph("\uf46e");
    /// <summary>Official icon: <c>trophy</c></summary>
    public static string? Trophy => Glyph("\uea23");
    /// <summary>Official icon: <c>troubleshoot</c></summary>
    public static string? Troubleshoot => Glyph("\ue1d2");
    /// <summary>Official icon: <c>try</c></summary>
    public static string? Try => Glyph("\uf07c");
    /// <summary>Official icon: <c>tsunami</c></summary>
    public static string? Tsunami => Glyph("\uebd8");
    /// <summary>Official icon: <c>tsv</c></summary>
    public static string? Tsv => Glyph("\ue6d6");
    /// <summary>Official icon: <c>tty</c></summary>
    public static string? Tty => Glyph("\uf1aa");
    /// <summary>Official icon: <c>tune</c></summary>
    public static string? Tune => Glyph("\ue429");
    /// <summary>Official icon: <c>tungsten</c></summary>
    public static string? Tungsten => Glyph("\uf07d");
    /// <summary>Official icon: <c>turn_left</c></summary>
    public static string? TurnLeft => Glyph("\ueba6");
    /// <summary>Official icon: <c>turn_right</c></summary>
    public static string? TurnRight => Glyph("\uebab");
    /// <summary>Official icon: <c>turn_sharp_left</c></summary>
    public static string? TurnSharpLeft => Glyph("\ueba7");
    /// <summary>Official icon: <c>turn_sharp_right</c></summary>
    public static string? TurnSharpRight => Glyph("\uebaa");
    /// <summary>Official icon: <c>turn_slight_left</c></summary>
    public static string? TurnSlightLeft => Glyph("\ueba4");
    /// <summary>Official icon: <c>turn_slight_right</c></summary>
    public static string? TurnSlightRight => Glyph("\ueb9a");
    /// <summary>Official icon: <c>turned_in</c></summary>
    public static string? TurnedIn => Glyph("\ue8e7");
    /// <summary>Official icon: <c>turned_in_not</c></summary>
    public static string? TurnedInNot => Glyph("\ue8e7");
    /// <summary>Official icon: <c>tv</c></summary>
    public static string? Tv => Glyph("\ue63b");
    /// <summary>Official icon: <c>tv_displays</c></summary>
    public static string? TvDisplays => Glyph("\uf3ec");
    /// <summary>Official icon: <c>tv_gen</c></summary>
    public static string? TvGen => Glyph("\ue830");
    /// <summary>Official icon: <c>tv_guide</c></summary>
    public static string? TvGuide => Glyph("\ue1dc");
    /// <summary>Official icon: <c>tv_next</c></summary>
    public static string? TvNext => Glyph("\uf3eb");
    /// <summary>Official icon: <c>tv_off</c></summary>
    public static string? TvOff => Glyph("\ue647");
    /// <summary>Official icon: <c>tv_options_edit_channels</c></summary>
    public static string? TvOptionsEditChannels => Glyph("\ue1dd");
    /// <summary>Official icon: <c>tv_options_input_settings</c></summary>
    public static string? TvOptionsInputSettings => Glyph("\ue1de");
    /// <summary>Official icon: <c>tv_remote</c></summary>
    public static string? TvRemote => Glyph("\uf5d9");
    /// <summary>Official icon: <c>tv_signin</c></summary>
    public static string? TvSignin => Glyph("\ue71b");
    /// <summary>Official icon: <c>tv_with_assistant</c></summary>
    public static string? TvWithAssistant => Glyph("\ue785");
    /// <summary>Official icon: <c>two_pager</c></summary>
    public static string? TwoPager => Glyph("\uf51f");
    /// <summary>Official icon: <c>two_pager_store</c></summary>
    public static string? TwoPagerStore => Glyph("\uf3c4");
    /// <summary>Official icon: <c>two_wheeler</c></summary>
    public static string? TwoWheeler => Glyph("\ue9f9");
    /// <summary>Official icon: <c>type_specimen</c></summary>
    public static string? TypeSpecimen => Glyph("\uf8f0");
    /// <summary>Official icon: <c>u_turn_left</c></summary>
    public static string? UTurnLeft => Glyph("\ueba1");
    /// <summary>Official icon: <c>u_turn_right</c></summary>
    public static string? UTurnRight => Glyph("\ueba2");
    /// <summary>Official icon: <c>udon</c></summary>
    public static string? Udon => Glyph("\uef32");
    /// <summary>Official icon: <c>ulna_radius</c></summary>
    public static string? UlnaRadius => Glyph("\uf89d");
    /// <summary>Official icon: <c>ulna_radius_alt</c></summary>
    public static string? UlnaRadiusAlt => Glyph("\uf89e");
    /// <summary>Official icon: <c>umbrella</c></summary>
    public static string? Umbrella => Glyph("\uf1ad");
    /// <summary>Official icon: <c>unarchive</c></summary>
    public static string? Unarchive => Glyph("\ue169");
    /// <summary>Official icon: <c>undereye</c></summary>
    public static string? Undereye => Glyph("\ueeb1");
    /// <summary>Official icon: <c>undo</c></summary>
    public static string? Undo => Glyph("\ue166");
    /// <summary>Official icon: <c>unfold_less</c></summary>
    public static string? UnfoldLess => Glyph("\ue5d6");
    /// <summary>Official icon: <c>unfold_less_double</c></summary>
    public static string? UnfoldLessDouble => Glyph("\uf8cf");
    /// <summary>Official icon: <c>unfold_more</c></summary>
    public static string? UnfoldMore => Glyph("\ue5d7");
    /// <summary>Official icon: <c>unfold_more_double</c></summary>
    public static string? UnfoldMoreDouble => Glyph("\uf8d0");
    /// <summary>Official icon: <c>ungroup</c></summary>
    public static string? Ungroup => Glyph("\uf731");
    /// <summary>Official icon: <c>universal_currency</c></summary>
    public static string? UniversalCurrency => Glyph("\ue9fa");
    /// <summary>Official icon: <c>universal_currency_alt</c></summary>
    public static string? UniversalCurrencyAlt => Glyph("\ue734");
    /// <summary>Official icon: <c>universal_local</c></summary>
    public static string? UniversalLocal => Glyph("\ue9fb");
    /// <summary>Official icon: <c>unknown_2</c></summary>
    public static string? Unknown2 => Glyph("\uf49f");
    /// <summary>Official icon: <c>unknown_5</c></summary>
    public static string? Unknown5 => Glyph("\ue6a5");
    /// <summary>Official icon: <c>unknown_7</c></summary>
    public static string? Unknown7 => Glyph("\uf49e");
    /// <summary>Official icon: <c>unknown_document</c></summary>
    public static string? UnknownDocument => Glyph("\uf804");
    /// <summary>Official icon: <c>unknown_med</c></summary>
    public static string? UnknownMed => Glyph("\ueabd");
    /// <summary>Official icon: <c>unlicense</c></summary>
    public static string? Unlicense => Glyph("\ueb05");
    /// <summary>Official icon: <c>unpaved_road</c></summary>
    public static string? UnpavedRoad => Glyph("\uf46d");
    /// <summary>Official icon: <c>unpin</c></summary>
    public static string? Unpin => Glyph("\ue6f9");
    /// <summary>Official icon: <c>unpublished</c></summary>
    public static string? Unpublished => Glyph("\uf236");
    /// <summary>Official icon: <c>unsubscribe</c></summary>
    public static string? Unsubscribe => Glyph("\ue0eb");
    /// <summary>Official icon: <c>upcoming</c></summary>
    public static string? Upcoming => Glyph("\uf07e");
    /// <summary>Official icon: <c>update</c></summary>
    public static string? Update => Glyph("\ue923");
    /// <summary>Official icon: <c>update_disabled</c></summary>
    public static string? UpdateDisabled => Glyph("\ue075");
    /// <summary>Official icon: <c>upgrade</c></summary>
    public static string? Upgrade => Glyph("\uf0fb");
    /// <summary>Official icon: <c>upi_pay</c></summary>
    public static string? UpiPay => Glyph("\uf3cf");
    /// <summary>Official icon: <c>upload</c></summary>
    public static string? Upload => Glyph("\uf09b");
    /// <summary>Official icon: <c>upload_2</c></summary>
    public static string? Upload2 => Glyph("\uf521");
    /// <summary>Official icon: <c>upload_file</c></summary>
    public static string? UploadFile => Glyph("\ue9fc");
    /// <summary>Official icon: <c>uppercase</c></summary>
    public static string? Uppercase => Glyph("\uf488");
    /// <summary>Official icon: <c>urology</c></summary>
    public static string? Urology => Glyph("\ue137");
    /// <summary>Official icon: <c>usb</c></summary>
    public static string? Usb => Glyph("\ue1e0");
    /// <summary>Official icon: <c>usb_off</c></summary>
    public static string? UsbOff => Glyph("\ue4fa");
    /// <summary>Official icon: <c>user_attributes</c></summary>
    public static string? UserAttributes => Glyph("\ue708");
    /// <summary>Official icon: <c>vaccines</c></summary>
    public static string? Vaccines => Glyph("\ue138");
    /// <summary>Official icon: <c>vacuum</c></summary>
    public static string? Vacuum => Glyph("\uefc5");
    /// <summary>Official icon: <c>vacuum_2</c></summary>
    public static string? Vacuum2 => Glyph("\U000FFF6D");
    /// <summary>Official icon: <c>vacuum_2_on</c></summary>
    public static string? Vacuum2On => Glyph("\U000FFF6E");
    /// <summary>Official icon: <c>valve</c></summary>
    public static string? Valve => Glyph("\ue224");
    /// <summary>Official icon: <c>vape_free</c></summary>
    public static string? VapeFree => Glyph("\uebc6");
    /// <summary>Official icon: <c>vaping_rooms</c></summary>
    public static string? VapingRooms => Glyph("\uebcf");
    /// <summary>Official icon: <c>variable_add</c></summary>
    public static string? VariableAdd => Glyph("\uf51e");
    /// <summary>Official icon: <c>variable_insert</c></summary>
    public static string? VariableInsert => Glyph("\uf51d");
    /// <summary>Official icon: <c>variable_remove</c></summary>
    public static string? VariableRemove => Glyph("\uf51c");
    /// <summary>Official icon: <c>variables</c></summary>
    public static string? Variables => Glyph("\uf851");
    /// <summary>Official icon: <c>ventilator</c></summary>
    public static string? Ventilator => Glyph("\ue139");
    /// <summary>Official icon: <c>verified</c></summary>
    public static string? Verified => Glyph("\uef76");
    /// <summary>Official icon: <c>verified_off</c></summary>
    public static string? VerifiedOff => Glyph("\uf30e");
    /// <summary>Official icon: <c>verified_user</c></summary>
    public static string? VerifiedUser => Glyph("\uf013");
    /// <summary>Official icon: <c>vertical_align_bottom</c></summary>
    public static string? VerticalAlignBottom => Glyph("\ue258");
    /// <summary>Official icon: <c>vertical_align_center</c></summary>
    public static string? VerticalAlignCenter => Glyph("\ue259");
    /// <summary>Official icon: <c>vertical_align_top</c></summary>
    public static string? VerticalAlignTop => Glyph("\ue25a");
    /// <summary>Official icon: <c>vertical_distribute</c></summary>
    public static string? VerticalDistribute => Glyph("\ue076");
    /// <summary>Official icon: <c>vertical_shades</c></summary>
    public static string? VerticalShades => Glyph("\uec0e");
    /// <summary>Official icon: <c>vertical_shades_closed</c></summary>
    public static string? VerticalShadesClosed => Glyph("\uec0d");
    /// <summary>Official icon: <c>vertical_split</c></summary>
    public static string? VerticalSplit => Glyph("\ue949");
    /// <summary>Official icon: <c>vibration</c></summary>
    public static string? Vibration => Glyph("\uf2cb");
    /// <summary>Official icon: <c>video_call</c></summary>
    public static string? VideoCall => Glyph("\ue070");
    /// <summary>Official icon: <c>video_camera_back</c></summary>
    public static string? VideoCameraBack => Glyph("\uf07f");
    /// <summary>Official icon: <c>video_camera_back_add</c></summary>
    public static string? VideoCameraBackAdd => Glyph("\uf40c");
    /// <summary>Official icon: <c>video_camera_front</c></summary>
    public static string? VideoCameraFront => Glyph("\uf080");
    /// <summary>Official icon: <c>video_camera_front_off</c></summary>
    public static string? VideoCameraFrontOff => Glyph("\uf83b");
    /// <summary>Official icon: <c>video_chat</c></summary>
    public static string? VideoChat => Glyph("\uf8a0");
    /// <summary>Official icon: <c>video_file</c></summary>
    public static string? VideoFile => Glyph("\ueb87");
    /// <summary>Official icon: <c>video_frame_copy</c></summary>
    public static string? VideoFrameCopy => Glyph("\U000FFF0D");
    /// <summary>Official icon: <c>video_frame_save</c></summary>
    public static string? VideoFrameSave => Glyph("\U000FFF0C");
    /// <summary>Official icon: <c>video_label</c></summary>
    public static string? VideoLabel => Glyph("\ue071");
    /// <summary>Official icon: <c>video_library</c></summary>
    public static string? VideoLibrary => Glyph("\ue04a");
    /// <summary>Official icon: <c>video_search</c></summary>
    public static string? VideoSearch => Glyph("\uefc6");
    /// <summary>Official icon: <c>video_settings</c></summary>
    public static string? VideoSettings => Glyph("\uea75");
    /// <summary>Official icon: <c>video_stable</c></summary>
    public static string? VideoStable => Glyph("\uf081");
    /// <summary>Official icon: <c>video_template</c></summary>
    public static string? VideoTemplate => Glyph("\U000FFFD3");
    /// <summary>Official icon: <c>videocam</c></summary>
    public static string? Videocam => Glyph("\ue04b");
    /// <summary>Official icon: <c>videocam_alert</c></summary>
    public static string? VideocamAlert => Glyph("\uf390");
    /// <summary>Official icon: <c>videocam_off</c></summary>
    public static string? VideocamOff => Glyph("\ue04c");
    /// <summary>Official icon: <c>videogame_asset</c></summary>
    public static string? VideogameAsset => Glyph("\ue338");
    /// <summary>Official icon: <c>videogame_asset_off</c></summary>
    public static string? VideogameAssetOff => Glyph("\ue500");
    /// <summary>Official icon: <c>view_agenda</c></summary>
    public static string? ViewAgenda => Glyph("\ue8e9");
    /// <summary>Official icon: <c>view_apps</c></summary>
    public static string? ViewApps => Glyph("\uf376");
    /// <summary>Official icon: <c>view_array</c></summary>
    public static string? ViewArray => Glyph("\ue8ea");
    /// <summary>Official icon: <c>view_carousel</c></summary>
    public static string? ViewCarousel => Glyph("\ue8eb");
    /// <summary>Official icon: <c>view_column</c></summary>
    public static string? ViewColumn => Glyph("\ue8ec");
    /// <summary>Official icon: <c>view_column_2</c></summary>
    public static string? ViewColumn2 => Glyph("\uf847");
    /// <summary>Official icon: <c>view_comfy</c></summary>
    public static string? ViewComfy => Glyph("\ue42a");
    /// <summary>Official icon: <c>view_comfy_alt</c></summary>
    public static string? ViewComfyAlt => Glyph("\ueb73");
    /// <summary>Official icon: <c>view_compact</c></summary>
    public static string? ViewCompact => Glyph("\ue42b");
    /// <summary>Official icon: <c>view_compact_alt</c></summary>
    public static string? ViewCompactAlt => Glyph("\ueb74");
    /// <summary>Official icon: <c>view_cozy</c></summary>
    public static string? ViewCozy => Glyph("\ueb75");
    /// <summary>Official icon: <c>view_day</c></summary>
    public static string? ViewDay => Glyph("\ue8ed");
    /// <summary>Official icon: <c>view_headline</c></summary>
    public static string? ViewHeadline => Glyph("\ue8ee");
    /// <summary>Official icon: <c>view_in_ar</c></summary>
    public static string? ViewInAr => Glyph("\uefc9");
    /// <summary>Official icon: <c>view_in_ar_new</c></summary>
    public static string? ViewInArNew => Glyph("\uefc9");
    /// <summary>Official icon: <c>view_in_ar_off</c></summary>
    public static string? ViewInArOff => Glyph("\uf61b");
    /// <summary>Official icon: <c>view_kanban</c></summary>
    public static string? ViewKanban => Glyph("\ueb7f");
    /// <summary>Official icon: <c>view_list</c></summary>
    public static string? ViewList => Glyph("\ue8ef");
    /// <summary>Official icon: <c>view_module</c></summary>
    public static string? ViewModule => Glyph("\ue8f0");
    /// <summary>Official icon: <c>view_object_track</c></summary>
    public static string? ViewObjectTrack => Glyph("\uf432");
    /// <summary>Official icon: <c>view_quilt</c></summary>
    public static string? ViewQuilt => Glyph("\ue8f1");
    /// <summary>Official icon: <c>view_real_size</c></summary>
    public static string? ViewRealSize => Glyph("\uf4c2");
    /// <summary>Official icon: <c>view_sidebar</c></summary>
    public static string? ViewSidebar => Glyph("\uf114");
    /// <summary>Official icon: <c>view_stream</c></summary>
    public static string? ViewStream => Glyph("\ue8f2");
    /// <summary>Official icon: <c>view_timeline</c></summary>
    public static string? ViewTimeline => Glyph("\ueb85");
    /// <summary>Official icon: <c>view_week</c></summary>
    public static string? ViewWeek => Glyph("\ue8f3");
    /// <summary>Official icon: <c>vignette</c></summary>
    public static string? Vignette => Glyph("\ue435");
    /// <summary>Official icon: <c>vignette_2</c></summary>
    public static string? Vignette2 => Glyph("\uf2b3");
    /// <summary>Official icon: <c>villa</c></summary>
    public static string? Villa => Glyph("\ue586");
    /// <summary>Official icon: <c>visibility</c></summary>
    public static string? Visibility => Glyph("\ue8f4");
    /// <summary>Official icon: <c>visibility_lock</c></summary>
    public static string? VisibilityLock => Glyph("\uf653");
    /// <summary>Official icon: <c>visibility_off</c></summary>
    public static string? VisibilityOff => Glyph("\ue8f5");
    /// <summary>Official icon: <c>vital_signs</c></summary>
    public static string? VitalSigns => Glyph("\ue650");
    /// <summary>Official icon: <c>vitals</c></summary>
    public static string? Vitals => Glyph("\ue13b");
    /// <summary>Official icon: <c>vo2_max</c></summary>
    public static string? Vo2Max => Glyph("\uf4aa");
    /// <summary>Official icon: <c>voice_chat</c></summary>
    public static string? VoiceChat => Glyph("\ue62e");
    /// <summary>Official icon: <c>voice_chat_off</c></summary>
    public static string? VoiceChatOff => Glyph("\ueebe");
    /// <summary>Official icon: <c>voice_over_off</c></summary>
    public static string? VoiceOverOff => Glyph("\ue94a");
    /// <summary>Official icon: <c>voice_selection</c></summary>
    public static string? VoiceSelection => Glyph("\uf58a");
    /// <summary>Official icon: <c>voice_selection_off</c></summary>
    public static string? VoiceSelectionOff => Glyph("\uf42c");
    /// <summary>Official icon: <c>voicemail</c></summary>
    public static string? Voicemail => Glyph("\ue0d9");
    /// <summary>Official icon: <c>voicemail_2</c></summary>
    public static string? Voicemail2 => Glyph("\uf352");
    /// <summary>Official icon: <c>volcano</c></summary>
    public static string? Volcano => Glyph("\uebda");
    /// <summary>Official icon: <c>volume_down</c></summary>
    public static string? VolumeDown => Glyph("\ue04d");
    /// <summary>Official icon: <c>volume_down_alt</c></summary>
    public static string? VolumeDownAlt => Glyph("\ue79c");
    /// <summary>Official icon: <c>volume_mute</c></summary>
    public static string? VolumeMute => Glyph("\ue04e");
    /// <summary>Official icon: <c>volume_off</c></summary>
    public static string? VolumeOff => Glyph("\ue04f");
    /// <summary>Official icon: <c>volume_up</c></summary>
    public static string? VolumeUp => Glyph("\ue050");
    /// <summary>Official icon: <c>volunteer_activism</c></summary>
    public static string? VolunteerActivism => Glyph("\uea70");
    /// <summary>Official icon: <c>voting_chip</c></summary>
    public static string? VotingChip => Glyph("\uf852");
    /// <summary>Official icon: <c>vpn_key</c></summary>
    public static string? VpnKey => Glyph("\ue0da");
    /// <summary>Official icon: <c>vpn_key_alert</c></summary>
    public static string? VpnKeyAlert => Glyph("\uf6cc");
    /// <summary>Official icon: <c>vpn_key_off</c></summary>
    public static string? VpnKeyOff => Glyph("\ueb7a");
    /// <summary>Official icon: <c>vpn_lock</c></summary>
    public static string? VpnLock => Glyph("\ue62f");
    /// <summary>Official icon: <c>vpn_lock_2</c></summary>
    public static string? VpnLock2 => Glyph("\uf350");
    /// <summary>Official icon: <c>vr180_create2d</c></summary>
    public static string? Vr180Create2d => Glyph("\uefca");
    /// <summary>Official icon: <c>vr180_create2d_off</c></summary>
    public static string? Vr180Create2dOff => Glyph("\uf571");
    /// <summary>Official icon: <c>vrpano</c></summary>
    public static string? Vrpano => Glyph("\uf082");
    /// <summary>Official icon: <c>walk_bike</c></summary>
    public static string? WalkBike => Glyph("\U000FFF01");
    /// <summary>Official icon: <c>wall_art</c></summary>
    public static string? WallArt => Glyph("\uefcb");
    /// <summary>Official icon: <c>wall_lamp</c></summary>
    public static string? WallLamp => Glyph("\ue2b4");
    /// <summary>Official icon: <c>wallet</c></summary>
    public static string? Wallet => Glyph("\uf8ff");
    /// <summary>Official icon: <c>wallpaper</c></summary>
    public static string? Wallpaper => Glyph("\ue1bc");
    /// <summary>Official icon: <c>wallpaper_slideshow</c></summary>
    public static string? WallpaperSlideshow => Glyph("\uf672");
    /// <summary>Official icon: <c>wand_shine</c></summary>
    public static string? WandShine => Glyph("\uf31f");
    /// <summary>Official icon: <c>wand_stars</c></summary>
    public static string? WandStars => Glyph("\uf31e");
    /// <summary>Official icon: <c>ward</c></summary>
    public static string? Ward => Glyph("\ue13c");
    /// <summary>Official icon: <c>warehouse</c></summary>
    public static string? Warehouse => Glyph("\uebb8");
    /// <summary>Official icon: <c>warning</c></summary>
    public static string? Warning => Glyph("\uf083");
    /// <summary>Official icon: <c>warning_amber</c></summary>
    public static string? WarningAmber => Glyph("\uf083");
    /// <summary>Official icon: <c>warning_off</c></summary>
    public static string? WarningOff => Glyph("\uf7ad");
    /// <summary>Official icon: <c>wash</c></summary>
    public static string? Wash => Glyph("\uf1b1");
    /// <summary>Official icon: <c>washoku</c></summary>
    public static string? Washoku => Glyph("\uf280");
    /// <summary>Official icon: <c>watch</c></summary>
    public static string? Watch => Glyph("\ue334");
    /// <summary>Official icon: <c>watch_alert</c></summary>
    public static string? WatchAlert => Glyph("\U000FFFD1");
    /// <summary>Official icon: <c>watch_arrow</c></summary>
    public static string? WatchArrow => Glyph("\uf2ca");
    /// <summary>Official icon: <c>watch_arrow_down</c></summary>
    public static string? WatchArrowDown => Glyph("\U000FFFCF");
    /// <summary>Official icon: <c>watch_button</c></summary>
    public static string? WatchButton => Glyph("\U000FFF20");
    /// <summary>Official icon: <c>watch_button_press</c></summary>
    public static string? WatchButtonPress => Glyph("\uf6aa");
    /// <summary>Official icon: <c>watch_check</c></summary>
    public static string? WatchCheck => Glyph("\uf468");
    /// <summary>Official icon: <c>watch_later</c></summary>
    public static string? WatchLater => Glyph("\uefd6");
    /// <summary>Official icon: <c>watch_lock</c></summary>
    public static string? WatchLock => Glyph("\ueee9");
    /// <summary>Official icon: <c>watch_off</c></summary>
    public static string? WatchOff => Glyph("\ueae3");
    /// <summary>Official icon: <c>watch_screentime</c></summary>
    public static string? WatchScreentime => Glyph("\uf6ae");
    /// <summary>Official icon: <c>watch_vibration</c></summary>
    public static string? WatchVibration => Glyph("\uf467");
    /// <summary>Official icon: <c>watch_wake</c></summary>
    public static string? WatchWake => Glyph("\uf6a9");
    /// <summary>Official icon: <c>water</c></summary>
    public static string? Water => Glyph("\uf084");
    /// <summary>Official icon: <c>water_bottle</c></summary>
    public static string? WaterBottle => Glyph("\uf69d");
    /// <summary>Official icon: <c>water_bottle_large</c></summary>
    public static string? WaterBottleLarge => Glyph("\uf69e");
    /// <summary>Official icon: <c>water_damage</c></summary>
    public static string? WaterDamage => Glyph("\uf203");
    /// <summary>Official icon: <c>water_do</c></summary>
    public static string? WaterDo => Glyph("\uf870");
    /// <summary>Official icon: <c>water_drop</c></summary>
    public static string? WaterDrop => Glyph("\ue798");
    /// <summary>Official icon: <c>water_drops</c></summary>
    public static string? WaterDrops => Glyph("\U000FFFA5");
    /// <summary>Official icon: <c>water_ec</c></summary>
    public static string? WaterEc => Glyph("\uf875");
    /// <summary>Official icon: <c>water_full</c></summary>
    public static string? WaterFull => Glyph("\uf6d6");
    /// <summary>Official icon: <c>water_heater</c></summary>
    public static string? WaterHeater => Glyph("\ue284");
    /// <summary>Official icon: <c>water_lock</c></summary>
    public static string? WaterLock => Glyph("\uf6ad");
    /// <summary>Official icon: <c>water_loss</c></summary>
    public static string? WaterLoss => Glyph("\uf6d5");
    /// <summary>Official icon: <c>water_lux</c></summary>
    public static string? WaterLux => Glyph("\uf874");
    /// <summary>Official icon: <c>water_medium</c></summary>
    public static string? WaterMedium => Glyph("\uf6d4");
    /// <summary>Official icon: <c>water_orp</c></summary>
    public static string? WaterOrp => Glyph("\uf878");
    /// <summary>Official icon: <c>water_ph</c></summary>
    public static string? WaterPh => Glyph("\uf87a");
    /// <summary>Official icon: <c>water_pump</c></summary>
    public static string? WaterPump => Glyph("\uf5d8");
    /// <summary>Official icon: <c>water_voc</c></summary>
    public static string? WaterVoc => Glyph("\uf87b");
    /// <summary>Official icon: <c>waterfall_chart</c></summary>
    public static string? WaterfallChart => Glyph("\uea00");
    /// <summary>Official icon: <c>waves</c></summary>
    public static string? Waves => Glyph("\ue176");
    /// <summary>Official icon: <c>waving_hand</c></summary>
    public static string? WavingHand => Glyph("\ue766");
    /// <summary>Official icon: <c>wb_auto</c></summary>
    public static string? WbAuto => Glyph("\ue42c");
    /// <summary>Official icon: <c>wb_cloudy</c></summary>
    public static string? WbCloudy => Glyph("\uf15c");
    /// <summary>Official icon: <c>wb_incandescent</c></summary>
    public static string? WbIncandescent => Glyph("\ue42e");
    /// <summary>Official icon: <c>wb_iridescent</c></summary>
    public static string? WbIridescent => Glyph("\uf07d");
    /// <summary>Official icon: <c>wb_shade</c></summary>
    public static string? WbShade => Glyph("\uea01");
    /// <summary>Official icon: <c>wb_sunny</c></summary>
    public static string? WbSunny => Glyph("\ue430");
    /// <summary>Official icon: <c>wb_twilight</c></summary>
    public static string? WbTwilight => Glyph("\ue1c6");
    /// <summary>Official icon: <c>wb_twilight_2</c></summary>
    public static string? WbTwilight2 => Glyph("\U000FFF1F");
    /// <summary>Official icon: <c>wc</c></summary>
    public static string? Wc => Glyph("\ue63d");
    /// <summary>Official icon: <c>weather_hail</c></summary>
    public static string? WeatherHail => Glyph("\uf67f");
    /// <summary>Official icon: <c>weather_mix</c></summary>
    public static string? WeatherMix => Glyph("\uf60b");
    /// <summary>Official icon: <c>weather_snowy</c></summary>
    public static string? WeatherSnowy => Glyph("\ue2cd");
    /// <summary>Official icon: <c>web</c></summary>
    public static string? Web => Glyph("\ue66a");
    /// <summary>Official icon: <c>web_asset</c></summary>
    public static string? WebAsset => Glyph("\ue069");
    /// <summary>Official icon: <c>web_asset_off</c></summary>
    public static string? WebAssetOff => Glyph("\uef47");
    /// <summary>Official icon: <c>web_stories</c></summary>
    public static string? WebStories => Glyph("\ue595");
    /// <summary>Official icon: <c>web_traffic</c></summary>
    public static string? WebTraffic => Glyph("\uea03");
    /// <summary>Official icon: <c>webhook</c></summary>
    public static string? Webhook => Glyph("\ueb92");
    /// <summary>Official icon: <c>weekend</c></summary>
    public static string? Weekend => Glyph("\ue16b");
    /// <summary>Official icon: <c>weight</c></summary>
    public static string? Weight => Glyph("\ue13d");
    /// <summary>Official icon: <c>west</c></summary>
    public static string? West => Glyph("\uf1e6");
    /// <summary>Official icon: <c>whatshot</c></summary>
    public static string? Whatshot => Glyph("\ue80e");
    /// <summary>Official icon: <c>wheat</c></summary>
    public static string? Wheat => Glyph("\U000FFFA4");
    /// <summary>Official icon: <c>wheelchair_pickup</c></summary>
    public static string? WheelchairPickup => Glyph("\uf1ab");
    /// <summary>Official icon: <c>where_to_vote</c></summary>
    public static string? WhereToVote => Glyph("\ue177");
    /// <summary>Official icon: <c>widget_medium</c></summary>
    public static string? WidgetMedium => Glyph("\uf3ba");
    /// <summary>Official icon: <c>widget_menu</c></summary>
    public static string? WidgetMenu => Glyph("\ueeb7");
    /// <summary>Official icon: <c>widget_small</c></summary>
    public static string? WidgetSmall => Glyph("\uf3b9");
    /// <summary>Official icon: <c>widget_width</c></summary>
    public static string? WidgetWidth => Glyph("\uf3b8");
    /// <summary>Official icon: <c>widgets</c></summary>
    public static string? Widgets => Glyph("\ue1bd");
    /// <summary>Official icon: <c>width</c></summary>
    public static string? Width => Glyph("\uf730");
    /// <summary>Official icon: <c>width_full</c></summary>
    public static string? WidthFull => Glyph("\uf8f5");
    /// <summary>Official icon: <c>width_normal</c></summary>
    public static string? WidthNormal => Glyph("\uf8f6");
    /// <summary>Official icon: <c>width_wide</c></summary>
    public static string? WidthWide => Glyph("\uf8f7");
    /// <summary>Official icon: <c>wifi</c></summary>
    public static string? Wifi => Glyph("\ue63e");
    /// <summary>Official icon: <c>wifi_1_bar</c></summary>
    public static string? Wifi1Bar => Glyph("\ue4ca");
    /// <summary>Official icon: <c>wifi_2_bar</c></summary>
    public static string? Wifi2Bar => Glyph("\ue4d9");
    /// <summary>Official icon: <c>wifi_add</c></summary>
    public static string? WifiAdd => Glyph("\uf7a8");
    /// <summary>Official icon: <c>wifi_calling</c></summary>
    public static string? WifiCalling => Glyph("\uef77");
    /// <summary>Official icon: <c>wifi_calling_1</c></summary>
    public static string? WifiCalling1 => Glyph("\uf0e7");
    /// <summary>Official icon: <c>wifi_calling_2</c></summary>
    public static string? WifiCalling2 => Glyph("\uf0f6");
    /// <summary>Official icon: <c>wifi_calling_3</c></summary>
    public static string? WifiCalling3 => Glyph("\uf0e7");
    /// <summary>Official icon: <c>wifi_calling_bar_1</c></summary>
    public static string? WifiCallingBar1 => Glyph("\uf44c");
    /// <summary>Official icon: <c>wifi_calling_bar_2</c></summary>
    public static string? WifiCallingBar2 => Glyph("\uf44b");
    /// <summary>Official icon: <c>wifi_calling_bar_3</c></summary>
    public static string? WifiCallingBar3 => Glyph("\uf44a");
    /// <summary>Official icon: <c>wifi_channel</c></summary>
    public static string? WifiChannel => Glyph("\ueb6a");
    /// <summary>Official icon: <c>wifi_device</c></summary>
    public static string? WifiDevice => Glyph("\U000FFF34");
    /// <summary>Official icon: <c>wifi_find</c></summary>
    public static string? WifiFind => Glyph("\ueb31");
    /// <summary>Official icon: <c>wifi_home</c></summary>
    public static string? WifiHome => Glyph("\uf671");
    /// <summary>Official icon: <c>wifi_lock</c></summary>
    public static string? WifiLock => Glyph("\ue1e1");
    /// <summary>Official icon: <c>wifi_notification</c></summary>
    public static string? WifiNotification => Glyph("\uf670");
    /// <summary>Official icon: <c>wifi_off</c></summary>
    public static string? WifiOff => Glyph("\ue648");
    /// <summary>Official icon: <c>wifi_password</c></summary>
    public static string? WifiPassword => Glyph("\ueb6b");
    /// <summary>Official icon: <c>wifi_protected_setup</c></summary>
    public static string? WifiProtectedSetup => Glyph("\uf0fc");
    /// <summary>Official icon: <c>wifi_proxy</c></summary>
    public static string? WifiProxy => Glyph("\uf7a7");
    /// <summary>Official icon: <c>wifi_tethering</c></summary>
    public static string? WifiTethering => Glyph("\ue1e2");
    /// <summary>Official icon: <c>wifi_tethering_error</c></summary>
    public static string? WifiTetheringError => Glyph("\uead9");
    /// <summary>Official icon: <c>wifi_tethering_off</c></summary>
    public static string? WifiTetheringOff => Glyph("\uf087");
    /// <summary>Official icon: <c>wind_power</c></summary>
    public static string? WindPower => Glyph("\uec0c");
    /// <summary>Official icon: <c>window</c></summary>
    public static string? Window => Glyph("\uf088");
    /// <summary>Official icon: <c>window_closed</c></summary>
    public static string? WindowClosed => Glyph("\ue77e");
    /// <summary>Official icon: <c>window_open</c></summary>
    public static string? WindowOpen => Glyph("\ue78c");
    /// <summary>Official icon: <c>window_sensor</c></summary>
    public static string? WindowSensor => Glyph("\ue2bb");
    /// <summary>Official icon: <c>windshield_defrost_auto</c></summary>
    public static string? WindshieldDefrostAuto => Glyph("\uf248");
    /// <summary>Official icon: <c>windshield_defrost_front</c></summary>
    public static string? WindshieldDefrostFront => Glyph("\uf32a");
    /// <summary>Official icon: <c>windshield_defrost_rear</c></summary>
    public static string? WindshieldDefrostRear => Glyph("\uf329");
    /// <summary>Official icon: <c>windshield_heat_front</c></summary>
    public static string? WindshieldHeatFront => Glyph("\uf328");
    /// <summary>Official icon: <c>wine_bar</c></summary>
    public static string? WineBar => Glyph("\uf1e8");
    /// <summary>Official icon: <c>woman</c></summary>
    public static string? Woman => Glyph("\ue13e");
    /// <summary>Official icon: <c>woman_2</c></summary>
    public static string? Woman2 => Glyph("\uf8e7");
    /// <summary>Official icon: <c>work</c></summary>
    public static string? Work => Glyph("\ue943");
    /// <summary>Official icon: <c>work_alert</c></summary>
    public static string? WorkAlert => Glyph("\uf5f7");
    /// <summary>Official icon: <c>work_history</c></summary>
    public static string? WorkHistory => Glyph("\uec09");
    /// <summary>Official icon: <c>work_off</c></summary>
    public static string? WorkOff => Glyph("\ue942");
    /// <summary>Official icon: <c>work_outline</c></summary>
    public static string? WorkOutline => Glyph("\ue943");
    /// <summary>Official icon: <c>work_update</c></summary>
    public static string? WorkUpdate => Glyph("\uf5f8");
    /// <summary>Official icon: <c>workflow</c></summary>
    public static string? Workflow => Glyph("\uea04");
    /// <summary>Official icon: <c>workspace_premium</c></summary>
    public static string? WorkspacePremium => Glyph("\ue7af");
    /// <summary>Official icon: <c>workspaces</c></summary>
    public static string? Workspaces => Glyph("\uea0f");
    /// <summary>Official icon: <c>workspaces_outline</c></summary>
    public static string? WorkspacesOutline => Glyph("\uea0f");
    /// <summary>Official icon: <c>wounds_injuries</c></summary>
    public static string? WoundsInjuries => Glyph("\ue13f");
    /// <summary>Official icon: <c>wrap_text</c></summary>
    public static string? WrapText => Glyph("\ue25b");
    /// <summary>Official icon: <c>wrist</c></summary>
    public static string? Wrist => Glyph("\uf69c");
    /// <summary>Official icon: <c>wrong_location</c></summary>
    public static string? WrongLocation => Glyph("\uef78");
    /// <summary>Official icon: <c>wysiwyg</c></summary>
    public static string? Wysiwyg => Glyph("\uf1c3");
    /// <summary>Official icon: <c>x_circle</c></summary>
    public static string? XCircle => Glyph("\ue349");
    /// <summary>Official icon: <c>y_circle</c></summary>
    public static string? YCircle => Glyph("\ueec5");
    /// <summary>Official icon: <c>yakitori</c></summary>
    public static string? Yakitori => Glyph("\uef31");
    /// <summary>Official icon: <c>yard</c></summary>
    public static string? Yard => Glyph("\uf089");
    /// <summary>Official icon: <c>yoshoku</c></summary>
    public static string? Yoshoku => Glyph("\uf27f");
    /// <summary>Official icon: <c>your_trips</c></summary>
    public static string? YourTrips => Glyph("\ueb2b");
    /// <summary>Official icon: <c>youtube_activity</c></summary>
    public static string? YoutubeActivity => Glyph("\uf85a");
    /// <summary>Official icon: <c>youtube_searched_for</c></summary>
    public static string? YoutubeSearchedFor => Glyph("\ue8fa");
    /// <summary>Official icon: <c>zone_person_alert</c></summary>
    public static string? ZonePersonAlert => Glyph("\ue781");
    /// <summary>Official icon: <c>zone_person_idle</c></summary>
    public static string? ZonePersonIdle => Glyph("\ue77a");
    /// <summary>Official icon: <c>zone_person_urgent</c></summary>
    public static string? ZonePersonUrgent => Glyph("\ue788");
    /// <summary>Official icon: <c>zoom_in</c></summary>
    public static string? ZoomIn => Glyph("\ue8ff");
    /// <summary>Official icon: <c>zoom_in_map</c></summary>
    public static string? ZoomInMap => Glyph("\ueb2d");
    /// <summary>Official icon: <c>zoom_out</c></summary>
    public static string? ZoomOut => Glyph("\ue900");
    /// <summary>Official icon: <c>zoom_out_map</c></summary>
    public static string? ZoomOutMap => Glyph("\ue56b");
    /// <summary>Official icon: <c>10k</c></summary>
    public static string? _10k => Glyph("\ue951");
    /// <summary>Official icon: <c>10mp</c></summary>
    public static string? _10mp => Glyph("\ue952");
    /// <summary>Official icon: <c>11mp</c></summary>
    public static string? _11mp => Glyph("\ue953");
    /// <summary>Official icon: <c>123</c></summary>
    public static string? _123 => Glyph("\ueb8d");
    /// <summary>Official icon: <c>12mp</c></summary>
    public static string? _12mp => Glyph("\ue954");
    /// <summary>Official icon: <c>13mp</c></summary>
    public static string? _13mp => Glyph("\ue955");
    /// <summary>Official icon: <c>14mp</c></summary>
    public static string? _14mp => Glyph("\ue956");
    /// <summary>Official icon: <c>15mp</c></summary>
    public static string? _15mp => Glyph("\ue957");
    /// <summary>Official icon: <c>16mp</c></summary>
    public static string? _16mp => Glyph("\ue958");
    /// <summary>Official icon: <c>17mp</c></summary>
    public static string? _17mp => Glyph("\ue959");
    /// <summary>Official icon: <c>18_up_rating</c></summary>
    public static string? _18UpRating => Glyph("\uf8fd");
    /// <summary>Official icon: <c>18mp</c></summary>
    public static string? _18mp => Glyph("\ue95a");
    /// <summary>Official icon: <c>19mp</c></summary>
    public static string? _19mp => Glyph("\ue95b");
    /// <summary>Official icon: <c>1k</c></summary>
    public static string? _1k => Glyph("\ue95c");
    /// <summary>Official icon: <c>1k_plus</c></summary>
    public static string? _1kPlus => Glyph("\ue95d");
    /// <summary>Official icon: <c>1x_mobiledata</c></summary>
    public static string? _1xMobiledata => Glyph("\uefcd");
    /// <summary>Official icon: <c>1x_mobiledata_badge</c></summary>
    public static string? _1xMobiledataBadge => Glyph("\uf7f1");
    /// <summary>Official icon: <c>20mp</c></summary>
    public static string? _20mp => Glyph("\ue95e");
    /// <summary>Official icon: <c>21mp</c></summary>
    public static string? _21mp => Glyph("\ue95f");
    /// <summary>Official icon: <c>22mp</c></summary>
    public static string? _22mp => Glyph("\ue960");
    /// <summary>Official icon: <c>23mp</c></summary>
    public static string? _23mp => Glyph("\ue961");
    /// <summary>Official icon: <c>24fps_select</c></summary>
    public static string? _24fpsSelect => Glyph("\uf3f2");
    /// <summary>Official icon: <c>24mp</c></summary>
    public static string? _24mp => Glyph("\ue962");
    /// <summary>Official icon: <c>2d</c></summary>
    public static string? _2d => Glyph("\uef37");
    /// <summary>Official icon: <c>2d_2</c></summary>
    public static string? _2d2 => Glyph("\U000FFF0E");
    /// <summary>Official icon: <c>2k</c></summary>
    public static string? _2k => Glyph("\ue963");
    /// <summary>Official icon: <c>2k_plus</c></summary>
    public static string? _2kPlus => Glyph("\ue964");
    /// <summary>Official icon: <c>2mp</c></summary>
    public static string? _2mp => Glyph("\ue965");
    /// <summary>Official icon: <c>30fps</c></summary>
    public static string? _30fps => Glyph("\uefce");
    /// <summary>Official icon: <c>30fps_select</c></summary>
    public static string? _30fpsSelect => Glyph("\uefcf");
    /// <summary>Official icon: <c>360</c></summary>
    public static string? _360 => Glyph("\ue577");
    /// <summary>Official icon: <c>3d</c></summary>
    public static string? _3d => Glyph("\ued38");
    /// <summary>Official icon: <c>3d_2</c></summary>
    public static string? _3d2 => Glyph("\U000FFF0F");
    /// <summary>Official icon: <c>3d_rotation</c></summary>
    public static string? _3dRotation => Glyph("\ue84d");
    /// <summary>Official icon: <c>3g_mobiledata</c></summary>
    public static string? _3gMobiledata => Glyph("\uefd0");
    /// <summary>Official icon: <c>3g_mobiledata_badge</c></summary>
    public static string? _3gMobiledataBadge => Glyph("\uf7f0");
    /// <summary>Official icon: <c>3k</c></summary>
    public static string? _3k => Glyph("\ue966");
    /// <summary>Official icon: <c>3k_plus</c></summary>
    public static string? _3kPlus => Glyph("\ue967");
    /// <summary>Official icon: <c>3mp</c></summary>
    public static string? _3mp => Glyph("\ue968");
    /// <summary>Official icon: <c>3p</c></summary>
    public static string? _3p => Glyph("\uefd1");
    /// <summary>Official icon: <c>4g_mobiledata</c></summary>
    public static string? _4gMobiledata => Glyph("\uefd2");
    /// <summary>Official icon: <c>4g_mobiledata_badge</c></summary>
    public static string? _4gMobiledataBadge => Glyph("\uf7ef");
    /// <summary>Official icon: <c>4g_plus_mobiledata</c></summary>
    public static string? _4gPlusMobiledata => Glyph("\uefd3");
    /// <summary>Official icon: <c>4k</c></summary>
    public static string? _4k => Glyph("\ue072");
    /// <summary>Official icon: <c>4k_plus</c></summary>
    public static string? _4kPlus => Glyph("\ue969");
    /// <summary>Official icon: <c>4mp</c></summary>
    public static string? _4mp => Glyph("\ue96a");
    /// <summary>Official icon: <c>50mp</c></summary>
    public static string? _50mp => Glyph("\uf6f3");
    /// <summary>Official icon: <c>5g</c></summary>
    public static string? _5g => Glyph("\uef38");
    /// <summary>Official icon: <c>5g_mobiledata_badge</c></summary>
    public static string? _5gMobiledataBadge => Glyph("\uf7ee");
    /// <summary>Official icon: <c>5k</c></summary>
    public static string? _5k => Glyph("\ue96b");
    /// <summary>Official icon: <c>5k_plus</c></summary>
    public static string? _5kPlus => Glyph("\ue96c");
    /// <summary>Official icon: <c>5mp</c></summary>
    public static string? _5mp => Glyph("\ue96d");
    /// <summary>Official icon: <c>60fps</c></summary>
    public static string? _60fps => Glyph("\uefd4");
    /// <summary>Official icon: <c>60fps_select</c></summary>
    public static string? _60fpsSelect => Glyph("\uefd5");
    /// <summary>Official icon: <c>6_ft_apart</c></summary>
    public static string? _6FtApart => Glyph("\uf21e");
    /// <summary>Official icon: <c>6k</c></summary>
    public static string? _6k => Glyph("\ue96e");
    /// <summary>Official icon: <c>6k_plus</c></summary>
    public static string? _6kPlus => Glyph("\ue96f");
    /// <summary>Official icon: <c>6mp</c></summary>
    public static string? _6mp => Glyph("\ue970");
    /// <summary>Official icon: <c>7k</c></summary>
    public static string? _7k => Glyph("\ue971");
    /// <summary>Official icon: <c>7k_plus</c></summary>
    public static string? _7kPlus => Glyph("\ue972");
    /// <summary>Official icon: <c>7mp</c></summary>
    public static string? _7mp => Glyph("\ue973");
    /// <summary>Official icon: <c>8k</c></summary>
    public static string? _8k => Glyph("\ue974");
    /// <summary>Official icon: <c>8k_plus</c></summary>
    public static string? _8kPlus => Glyph("\ue975");
    /// <summary>Official icon: <c>8mp</c></summary>
    public static string? _8mp => Glyph("\ue976");
    /// <summary>Official icon: <c>9k</c></summary>
    public static string? _9k => Glyph("\ue977");
    /// <summary>Official icon: <c>9k_plus</c></summary>
    public static string? _9kPlus => Glyph("\ue978");
    /// <summary>Official icon: <c>9mp</c></summary>
    public static string? _9mp => Glyph("\ue979");
}
