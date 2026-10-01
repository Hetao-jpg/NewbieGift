namespace NewbieGift;

internal static class CatalogIds
{
    // 由 items_catalog.md 全量导出，共 584 项。
    // 按分类分组，方便维护。
    public static readonly string[] All =
    {
        // ==================== 杂项（170） ====================
        "advanced_flux_agent", "aquascan", "aug_id_guide", "be_47_love_cup", "be_brewing_manual",
        "be_clear_water_tank", "be_compact_smuggler_bay", "be_contraband_perfume", "be_low_blindbox", "be_mid_blindbox",
        "be_multi_storage", "be_ninglu_filter", "be_premium_blindbox", "be_qingyun_filter", "be_wine_blindbox",
        "be_xuxu_sweet_cup", "be_xuxu_warm_cup", "be_yingfei_filter", "black_lamp", "blank_keycard",
        "blood_bag", "blue_blood_bag", "c4", "c4_set", "card_chem",
        "card_mining", "card_pharma", "card_rev", "cassette1", "cassette1d",
        "cassette1l", "cassette2", "cassette2d", "cassette2l", "cassette3",
        "cassette3d", "cassette3l", "cassette4", "cassette4d", "cassette4l",
        "cassette5", "cassette5d", "cassette5l", "cassette6", "cassette6d",
        "cassette6l", "cassette7", "cassette7d", "cassette7l", "cassette_player",
        "cat_bar", "cert1", "cigarette_color", "cmd_keycard", "common_chemical",
        "common_electronic", "common_medical", "crowbar", "cup_noodle", "desequencer",
        "dossier", "drain", "dream_dust", "empty_nuclear_waste_barrel", "energy_credit",
        "energy_credit_breeder", "energy_credit_ext", "energy_drink", "eng_keycard", "exchange_directive",
        "fanny_pack", "fat_meat", "faucet", "fce_chef_cookbook", "fce_cooking_wine",
        "fce_cuisine_certificate", "fce_cuisine_manual", "fce_dark_soy", "fce_light_soy", "fce_sugar",
        "fce_vinegar", "flashlight", "flux_agent", "furnace", "galaxy_blend",
        "gene_scanner", "gf_fish_mince_bait", "gf_glass_bait", "gf_rat_dung_bait", "gf_shiny_bait",
        "glass_shard", "glass_shard_shiv", "hand", "handnote", "hatchet",
        "joe_card", "kitchen_cleaver", "labeler", "labeler_blue", "li_eat_snackbar",
        "magnifier", "mentor_contract", "mirage_projector", "moisture_farm", "mouse_trap",
        "neuroactive_perfume", "newspaper", "nightmare_dust", "node_medium", "node_small",
        "nuclear_waste", "nudka", "nutrient_tablet", "packet_red_cigarette", "pack_condom",
        "paper_towel", "pheromone_perfume", "pipe_weapon", "plasma_fuel", "plastic_bag",
        "portable_water_purifier", "postit", "powerblock", "processed_cheese", "processed_juice",
        "processed_meat", "processed_milk", "rare_electronic", "raw_meat", "recharger",
        "restricted_chemical", "restricted_medical", "rev_codeword", "rubbing_alcohol", "salve",
        "satchel", "scav_token", "sci_keycard", "screwdriver", "security_alarm",
        "sec_keycard", "ser_keycard", "shampoo", "skincare_cream", "small_raw_meat",
        "smelling_salt", "soda_red", "stun_baton", "sup_keycard", "tackle_shop_processing_manual",
        "tackle_shop_willy_card", "tazer", "toilet_paper", "toothpaste", "turbo_booster",
        "turbo_booster_adv", "tutorial_page", "uv_filter", "wanted_paper", "water_filter",
        "water_filter_adv", "water_jug", "water_pitcher", "water_purifier", "water_tablet",
        "water_test_strip", "welder", "wire", "wire_cutter", "zerochew",

        // ==================== 机器（12） ====================
        "printer_plastic", "system_capped_neural_core", "system_module_corrupt", "system_module_eco", "system_module_efficiancy",
        "system_module_fineness", "system_module_overclock", "system_module_performance", "system_module_quality", "system_module_ruined",
        "system_module_streamlining", "vacuum_robot",

        // ==================== 模块（85） ====================
        "alarm_module_transmitter", "blank_module", "chem_module", "crypto_module_cmd", "crypto_module_eng",
        "crypto_module_med", "crypto_module_sec", "crypto_module_ser", "crypto_module_sup", "custom_storage_box",
        "furnace_module_blast", "furnace_module_junk", "gene_edit_probe", "gene_edit_probe_box", "keycard_pack",
        "module_extractor", "module_extractor_advanced", "void_bead_storage", "wage_ai_generator", "wage_ai_module",
        "wage_girl", "wage_gu_machine", "wage_protector_core", "xiaowo_auction_bearing_set", "xiaowo_auction_black_decoder",
        "xiaowo_auction_black_diamond", "xiaowo_auction_blood_filter", "xiaowo_auction_calibrator", "xiaowo_auction_century_vintage", "xiaowo_auction_cipher_ledger",
        "xiaowo_auction_coilgun", "xiaowo_auction_container", "xiaowo_auction_control_core", "xiaowo_auction_crystal_wine_set", "xiaowo_auction_fabricator_core",
        "xiaowo_auction_forged_pass", "xiaowo_auction_ghost_identity", "xiaowo_auction_hemostatic_foam", "xiaowo_auction_master_key", "xiaowo_auction_microreactor",
        "xiaowo_auction_micro_engraving", "xiaowo_auction_nano_suture", "xiaowo_auction_neural_repair", "xiaowo_auction_orbital_watch", "xiaowo_auction_organ_case",
        "xiaowo_auction_organ_matrix", "xiaowo_auction_pocket_watch", "xiaowo_auction_regen_core", "xiaowo_auction_silent_bolt", "xiaowo_auction_supercoil",
        "xiaowo_auction_zero_g_perfume", "xiaowo_auction_zero_point", "xiaowo_kitchen_beverage_machine", "xiaowo_kitchen_black_market_banquet", "xiaowo_kitchen_cheese_noodles",
        "xiaowo_kitchen_cheese_patty", "xiaowo_kitchen_clear_fruit_drink", "xiaowo_kitchen_cookbook", "xiaowo_kitchen_cooking_oil", "xiaowo_kitchen_creamy_stew",
        "xiaowo_kitchen_dried_vegetables", "xiaowo_kitchen_flowerberry_drink", "xiaowo_kitchen_fridge", "xiaowo_kitchen_fried_fat", "xiaowo_kitchen_fried_steak",
        "xiaowo_kitchen_kitchen_platter", "xiaowo_kitchen_large_raw_meat", "xiaowo_kitchen_meat_stew", "xiaowo_kitchen_milky_coffee", "xiaowo_kitchen_milky_drink",
        "xiaowo_kitchen_mince_soup", "xiaowo_kitchen_night_shift_coffee", "xiaowo_kitchen_overdrive_tonic", "xiaowo_kitchen_plate", "xiaowo_kitchen_plate_bundle",
        "xiaowo_kitchen_pot", "xiaowo_kitchen_red_fruit_soda", "xiaowo_kitchen_seasoning_salt", "xiaowo_kitchen_small_meat_skewer", "xiaowo_kitchen_spicy_mince",
        "xiaowo_kitchen_starch", "xiaowo_kitchen_starry_flowerberry_special", "xiaowo_kitchen_stove_double", "xiaowo_kitchen_stove_single", "xiaowo_kitchen_vegetable_noodles",

        // ==================== 武器/配件（19） ====================
        "armor_lining", "flashbang_grenade", "glock_receiver", "gun_part", "handmade_pistol",
        "heavy_handmade_pistol", "heavy_pistol_ammo", "heavy_pistol_ammo_nl", "heavy_pistol_ammo_p", "printed_gun",
        "revolver", "shotgun", "small_pistol_ammo", "small_pistol_ammo_nl", "small_pistol_ammo_p",
        "smg", "smoke_grenade", "storage_bay_gun", "stun_gun",

        // ==================== 工具（15） ====================
        "aug_scanner", "combat_knife", "fce_cookware_grill", "fce_cookware_pan", "fce_cookware_pot",
        "fce_fishing_kitchen", "gf_carbon_fiber_fishing_rod", "gf_double_hook_fishing_rod", "gf_fishing_net", "gf_makeshift_fishing_rod",
        "gf_rusty_slicing_knife", "kitchen_knife", "metal_scanner", "skinning_knife", "surgery_tool",

        // ==================== 背包（5） ====================
        "backpack_large", "backpack_large_military", "backpack_medium", "backpack_medium_military", "backpack_small",

        // ==================== 材料（7） ====================
        "common_ore", "ingot_book", "metal_ingot", "nuts_metal", "rare_ore",
        "scrap_metal", "sign_material",

        // ==================== 酿酒（22） ====================
        "bottled_water", "bottled_water_premium", "bottle_hot_sauce", "bottle_printer", "card_brewer",
        "empty_beer_bottle", "gf_blazing_chili_pepper", "gf_chili_pepper", "gf_fish_oil", "gf_salt",
        "gf_super_salt", "large_bottled_water", "mini_bottle", "red_beer", "small_bottled_water",
        "wine_berry", "wine_book", "wine_bottle", "wine_superyeast", "wine_yeast",
        "wine_yeast_infinite", "wine_yeast_red",

        // ==================== 医疗（24） ====================
        "bandage_item", "birth_control_pill", "black_injector", "caffeine_pill", "fanta_pill",
        "hemostatic_bandage_item", "injector_guide", "large_purple_injector", "med_bottle_blue", "med_bottle_orange",
        "med_bottle_red", "med_bottle_small_pink", "med_bottle_violet", "med_box", "med_keycard",
        "phagimycin_pill", "pink_alt_injector", "pink_injector", "pure_white_injector", "serum_green",
        "serum_red", "topical_bandage_item", "unlicensed_phagimycin_pill", "zyanide_pill",

        // ==================== 种植/水培（13） ====================
        "bloomberry_seed", "dream_cap", "dream_cap_seed", "fillerweed", "fillerweed_seed",
        "hydroreed_seed", "kotton_fabric", "kotton_fiber", "kotton_seed", "nightmare_cap",
        "nutrifruit", "nutrifruit_seed", "nutrifruit_seed_packet",

        // ==================== 容器/设施（36） ====================
        "back_inv_save_bag", "beerCase", "box_cutter", "box_dispenser", "box_tampon",
        "eng_box", "evidence_box", "expedition_box", "fridge", "gf_canned_goods_rack",
        "gf_drying_rack", "gf_food_freezer", "gf_processing_station", "gf_sealed_can", "hidden_save_bag",
        "machine_bay", "machine_bay_ext", "makeshift_storage_bay", "module_bay_expansion_kit", "nutrient_tablet_container",
        "save_bag", "sec_box", "service_box", "showcase_save_bag", "smuggler_bay",
        "smuggler_bay_mini", "smuggler_bay_mod", "sold_save_bag", "storage_bay", "storage_bay_chem",
        "storage_bay_large", "test_gizmo", "toolbox", "water_tablet_container", "water_test_container",
        "wine_rack",

        // ==================== 文书/杂项（16） ====================
        "business_permit", "cigarette_guide", "expedition_log", "fuel_guide", "hand_guide",
        "intel_sec", "logo_checker", "procurement_slip", "sec_slip", "sign_household",
        "sign_medical", "sign_water", "slip", "stamp_guide", "tutorial_book",
        "water_guide",

        // ==================== 食物（147） ====================
        "bayberry_fruit", "bayberry_seed", "beis_icecream", "be_advanced_yeast_reactor", "be_basic_cert",
        "be_brewery_sign", "be_cyber_bag", "be_feimei_wine", "be_forbidden_yeast_reactor", "be_golden_yeast_reactor",
        "be_high_cert", "be_ideal_end_wine", "be_incubator", "be_laolishi_bag", "be_luofu_dream_wine",
        "be_magic_pig_wine", "be_mid_cert", "be_nanfeng_wine", "be_neon_dream_wine", "be_neon_illusion_wine",
        "be_nini_card", "be_recipe_1", "be_recipe_2", "be_recipe_3", "be_recipe_4",
        "be_recipe_5", "be_recipe_6", "be_recipe_7", "be_recipe_8", "be_sakura_bag",
        "be_sea_blue_wine", "be_shuangqing_wine", "be_wine_printer", "be_wuren_mooncake", "be_xuxu_basic_battery",
        "be_xuxu_high_battery", "be_xuxu_mid_battery", "chili_fruit", "chili_seed", "coffee_bean",
        "cucumber_fruit", "cucumber_seed", "fce_dish_bad_meal", "fce_dish_fish_box_meal", "fce_dish_recipe_boil_clear",
        "fce_dish_recipe_boil_soy", "fce_dish_recipe_fry_oilpacked", "fce_dish_recipe_fry_plain", "fce_dish_recipe_grill_hot", "fce_dish_recipe_grill_plain",
        "fce_dish_recipe_grill_salted", "fce_dish_recipe_grill_spicy", "fce_dish_recipe_grill_sugar", "fce_dish_recipe_pan_cheese", "fce_dish_recipe_pan_juice",
        "fce_dish_recipe_raw_berry", "fce_dish_recipe_raw_crystal", "fce_dish_recipe_raw_plain", "fce_dish_recipe_steam_berry", "fce_dish_recipe_steam_plain",
        "fce_dish_recipe_stew_dark", "fce_dish_recipe_stew_nutri", "fce_dish_recipe_stew_plain", "fce_dish_recipe_stew_star", "food_stamp",
        "gf_blue_crystal_fish", "gf_blue_crystal_fish_fillet", "gf_conductive_tendon", "gf_dried_blue_crystal_fish", "gf_dried_electric_eel",
        "gf_dried_glass_fish", "gf_dried_golden_fish", "gf_dried_horned_swordfish", "gf_dried_large_yellow_croaker", "gf_dried_purifier_fish",
        "gf_dried_rainbow_fish", "gf_dried_ratfish", "gf_dried_redtail_fish", "gf_dried_sardine", "gf_dried_small_yellow_croaker",
        "gf_dried_volcanic_fish", "gf_electric_eel", "gf_electric_eel_fillet", "gf_glass_fish", "gf_gold",
        "gf_golden_fish", "gf_golden_fish_fillet", "gf_heat_gland", "gf_horned_swordfish", "gf_ice_crystal",
        "gf_large_yellow_croaker", "gf_oil_packed_fish", "gf_purification_sac", "gf_purifier_fish", "gf_purifier_fish_meat",
        "gf_purple_fillet", "gf_rainbow_fillet", "gf_rainbow_fish", "gf_ratfish", "gf_ratfish_fillet",
        "gf_redtail_fin", "gf_redtail_fish", "gf_redtail_fish_fillet", "gf_salted_fish", "gf_sardine",
        "gf_small_yellow_croaker", "gf_spicy_pickled_fish", "gf_star_fruit", "gf_swordfish_rostrum", "gf_transparent_membrane",
        "gf_volcanic_fish", "gf_volcanic_fish_fillet", "gf_yellow_croaker_fillet", "green_grape_fruit", "green_grape_seed",
        "jasmine_fruit", "jasmine_seed", "lemon_fruit", "lemon_seed", "lychee_fruit",
        "lychee_seed", "meal_cap", "morsel", "osmanthus_fruit", "osmanthus_seed",
        "ribwich", "ribwich_guide", "rice_fruit", "rice_seed", "sichuan_pepper_fruit",
        "sichuan_pepper_seed", "sign_food", "small_morsel", "stardust_berry_fruit", "stardust_berry_seed",
        "sugarcane_fruit", "sugarcane_seed", "void_plum_fruit", "void_plum_seed", "watertower",
        "watertower_tank", "wheat_fruit", "wheat_seed", "wormwood_fruit", "wormwood_seed",
        "yellow_grape_fruit", "yellow_grape_seed",

        // ==================== 养殖/牲畜（10） ====================
        "animal_cage", "feces_large", "feces_medium", "feces_small", "livestock_feed_dispenser",
        "rat", "scratcher", "scratcher_stack", "water_ration", "water_ration_small",

        // ==================== 废品（3） ====================
        "junk", "meat_scrap", "trashcan",
    };
}