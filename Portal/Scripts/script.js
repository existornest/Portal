


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
    var current;

    $($folder).click(function () {
        
        var li = $(this).parent();
        var liChildren = $(li).children("ul");

        
        liChildren.toggle();
        li.show();
        
    });

});