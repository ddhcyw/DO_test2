// 圖像廣場：莉亞與傳單事件
EXTERNAL show_flyer()      // 讓傳單出現在場景上
EXTERNAL get_flyer()       // 把傳單加入背包
EXTERNAL destroy_flyer()   // 讓場景上的傳單消失
EXTERNAL get_portfolio()   // 把作品集加入背包
EXTERNAL show_objective(content)
EXTERNAL change_scene(sceneName)
EXTERNAL unlock_plaza_door()
EXTERNAL add_clue(id)
EXTERNAL get_camera_item()





// 作品集偷偷任務完成，返回圖像廣場
=== plaza_lia_return ===
~ get_camera_item()
莉亞: 咦，是你啊！一切都還好嗎？感覺你們調查好久...
MAI: 莉亞小姐，請不用擔心！我們已經調查出一切的內容，不會再有有心人士盜用你的名義販賣盜版作品了！
MAI: （MAI把事件的來龍去脈全部都告訴莉亞...）
莉亞: 原來如此，幻影巷竟然有這些事，也無意間傷害了很多的居民...
莉亞: 但這麼困難的問題你們真的解決了！真的好感謝你們兩個幫助我！
莉亞: （莉亞露出燦爛的笑容）
莉亞: 為了報答你們，你有什麼想拍的都可以找我喔！我能幫你們拍出多漂亮的照片！

+ [莉亞小姐可以和我一起拍一張合照嗎？]
    莉亞: 當然可以呀！相機交給我，我幫你拍一張角度最好的照片！
    -> END

-> END
