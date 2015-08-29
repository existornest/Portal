


$(document).ready(function () {

    $liNav = $(".adv-li-nav");

    $liNav.hover(function () {
        //$(this).addClass("adv-active");

        $(this).css({ "border-top": '0 solid #700909' }).animate({
            borderWidth: 4,
            
        }, 200);

    }, function () {
        //$(this).removeClass("adv-active");

        $(this).animate({
            borderWidth: 0
        }, 200);

    });


    $folder = $("i");

    var visible = true;

    $folder.click(function () {

        if(visible)
        {
            //alert("HIDE");
            $(this).find("li").children().hide(1000);
            visible = false;
        }
        else {
            //alert("OPEN");
            $(this).find("li").children().show(1000);
            visible = true;
        }

        
    });

});