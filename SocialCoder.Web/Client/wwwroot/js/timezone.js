function InitializeTimezone()
{
    setTimeout(function(){
        $('.timezonepicker').timezonePicker({
            hoverText: function(e, data) {
                return (data.timezone + " (" + data.zonename + ")");
            },
            defaultValue: { value: "IN", attribute: "country"},
            selectBox: false,
            quickLink: [
                {
                    "PST":"PST",
                    "MST":"MST",
                    "CST":"CST",
                    "EST":"EST",
                    "GMT":"GMT",
                    "LONDON":"Europe/London",
                    "IST":"IST"
                }
            ]
        });

        $('.timezonepicker').on('map:country:clicked', function(){
            const data = GetSelectedTimeZoneInfo();
            console.log(data);
            OnTimeZoneValueChanged(data.Country, data.ZoneName);
            console.log("after");
        });
    },
    1000);
    
    
}

function GetSelectedTimeZoneInfo()
{
    const data = $('.timezonepicker').data('timezonePicker').getValue();
    
    if(!data) return null;
    
    return {
        "ZoneName": data[0]["zonename"],
        "Country": data[0]["country"]
    };
}
