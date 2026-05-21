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
EXTERNAL show_exit_icon()

~ show_objective("探索地圖，找到需要幫助的人吧！")

// 1. 第一次與莉亞對話
=== plaza_leah ===
莉亞: 歡迎來到莉亞的作品展！
莉亞: 我是攝影師莉亞，是社交區的攝影師，希望我的作品能夠帶給居民溫暖與歡笑！
莉亞: 這次我在圖像廣場一共展覽了三張作品，你可以去看看，可以的話也可以和我分享你的心得呦！
~ add_clue("LiaIntro")
~ show_objective("去看看莉亞的展覽吧！")
~ show_flyer()
~ show_objective("地上好像有奇怪的東西...")

-> END



// 2. 撿到奇怪的傳單
=== plaza_flyer_pickup ===

主角: 『利亞的商品展』……？莉亞也在幻影巷辦過展嗎？
主角: 總覺得哪裡怪怪的，去問問莉亞好了。

~ get_flyer()

~ destroy_flyer()
~ show_objective("把傳單拖曳給莉亞看看！")
-> END



// 3. 把傳單拿去給莉亞看，獲得作品集
=== plaza_leah_flyer ===
莉亞: 看完了嗎！覺得我的作品如何……咦？
莉亞: 『利亞的商品展』？上面好多都是我以前的作品……
莉亞: 但我從來沒有在幻影巷辦過展啊？太詭異了……！

MAI: 幫助居民的機會好像出現了！
MAI: 莉亞小姐，不用擔心，我們會去幫你看看，把這件事調查清楚的！

莉亞: 真的嗎？！你們真是大好人！
莉亞: 為了保險起見，我把我之前的作品集借給你好了，希望能在調查時派上用場。
莉亞: 真的很謝謝你們！
~ add_clue("LiaIntro")
~ get_portfolio()

MAI: 那我們現在就前往幻影巷吧！
~ show_objective("走到傳送門吧！")
~ show_exit_icon()
~ unlock_plaza_door()


-> END






// 作品集偷偷任務完成，返回圖像廣場
=== plaza_lia_return ===

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
