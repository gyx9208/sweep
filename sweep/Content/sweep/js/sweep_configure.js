
$(document).ready(function () {
    if (inProcess == 1)
        disableAllButton();
    else
        enableAllButton();

    $(".sweep-configure .btn-group button").click(function (e) {
        var b = $(e.target);
        if (! b.hasClass("active")) {
            b.siblings().each(function (index, object) {
                $(object).removeClass("active");
            });
            b.addClass("active");
            switch (b[0].id) {
                case "quick-mode":
                    $(".custom-div").slideUp();
                    quick = 1;
                    break;
                case "custom-mode":
                    $(".custom-div").slideDown();
                    quick = 0;
                    break;
                case "url-no":
                    url = 0;
                    break;
                case "url-yes":
                    url = 1;
                    break;
                case "follower-mode":
                    target = 0;
                    break;
                case "friend-mode":
                    target = 1;
                    break;
                default:
                    break;
            }
            
            
        }
    });

    $("#start-sweep").click(function (e) {
        if (inProcess == 0) {
            disableAllButton();
            allSlideUp();
            
            $.ajax({
                url: "Sweep/StartSweep",
                type: "POST",
                data: { QuickMode: quick, SelectTarget: target, IncludeUrl:url },
                success: function (data) {
                    if (data.result == 1) {
                        $(".progress-div").slideDown();
                        inProcess = 1;
                        getState();
                    } else {
                        enableAllButton();
                        inProcess = 0;
                    }
                }
            });

        } 
    });

    $("#end-sweep").click(function (e) {
        if (inProcess == 1) {
            $.ajax({
                url: "Sweep/EndSweep",
                type: "POST",
                success: function (data) {
                    if (data.result == 1) {
                        $(".progress-div").slideUp();
                        inProcess = 0;
                        enableAllButton();
                    }
                }
            });
        }
    });

    $("#clean-fans").click(function (e) {
        var list = {};
        list.length = 0;
        $(this).parent().siblings(".result-table-div").children(".user-gal.selected").each(function (index, e) {
            list[index] = $(e).attr("sinaID");
            list.length++;
            e.remove();
        });
        if (list.length > 0) {
            $.ajax({
                url: "Sweep/DeleteFans",
                type: "POST",
                data: list
            });
        }
    });

    getState();
    var timer = $.timer(1000,getState);
});

var getState=function(){
    if (inProcess == 1) {
        $.ajax({
            url: "Sweep/GetSweepState",
            success: function (data) {
                inProcess = data.stateid || 0;
                $(".progress-div .progress-text").text(data.state);
                if (inProcess == 1 || inProcess == 2) {
                    var w = data.finished / data.total * 100;
                    $(".progress-div .progress-bar").width(w + "%");
                    $(".progress-div .progress-bar").text(w.toFixed(1) + "%");
                }
                if (inProcess == 2) {
                    $.ajax({
                        url: "Sweep/GetSweepResult",
                        success: function (data) {
                            if (data.result == 0) {
                                var tardiv;
                                if (data.target == 0)
                                    tardiv = $(".result-fans");
                                else
                                    tardiv = $(".result-friends");

                                tardiv.find(".result-table-div").prepend("<div class=\"clearfix\"></div>");

                                tardiv.find(".total-count").text(data.totalCount);
                                tardiv.find(".official-waste-count").text(data.totalCount - data.realCount);
                                tardiv.find(".real-total-count").text(data.realCount);
                                tardiv.find(".waste-count").text(data.wasteCount);
                                var list = data.list;
                                for (var i = 0; i < list.length; i++) {
                                    var newItem = $("<div class=\"user-gal selected\"><div class=\"user-gal-img\"><img /></div><div class=\"user-gal-name\"><a target=\"_blank\"></a></div></div>");
                                    $(newItem).attr({ sinaID: list[i].id });
                                    $(newItem).find("img").attr({ src: list[i].imgurl });
                                    $(newItem).find(".user-gal-name").attr({ title: list[i].name });
                                    $(newItem).find("a").attr({ href: list[i].url });
                                    $(newItem).find("a").text(list[i].name);
                                    tardiv.find(".result-table-div").prepend($(newItem));
                                }
                                
                                tardiv.find(".result-table-div").prepend("<div class=\"checkbox-div\"><input type=\"checkbox\" checked=\"checked\"/>全选</div>");

                                tardiv.find(".checkbox-div input").change(function (e) {
                                    if ($(this).attr("checked") == "checked") {
                                        $(this).parent().siblings(".user-gal").addClass("selected");
                                    }
                                    else {
                                        $(this).parent().siblings(".user-gal").removeClass("selected");
                                    }
                                });

                                $(".user-gal-img").click(function (e) {
                                    if ($(this).parent().hasClass("selected")) {
                                        $(this).parent().removeClass("selected");
                                        $(this).parent().siblings(".checkbox-div").children("input").attr({ checked: false });
                                    } else {
                                        $(this).parent().addClass("selected");
                                    }
                                });

                                tardiv.slideDown();
                            } else {
                                //tobedone
                            }
                            enableAllButton();
                            inProcess = 0;
                        }
                    });
                    
                }
            }
        });
    }
};

var disableAllButton = function () {
    $(".sweep-configure button").attr({ disabled: "disabled" });
    $("#end-sweep").removeAttr("disabled");
}

var enableAllButton = function () {
    $(".sweep-configure button").removeAttr("disabled");
    $("#end-sweep").attr({ disabled: "disabled" });
}

var allSlideUp = function () {
    $(".progress-div").slideUp();
    $(".result-div").slideUp();
    $(".progress-div .progress-text").text("");
    $(".progress-div .progress-bar").width("0%");
    $(".progress-div .progress-bar").text("0%");
    $(".result-table-div").children().remove();
    $(".result-div").children(".dynamic").text("");
}