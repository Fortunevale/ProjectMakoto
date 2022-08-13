﻿namespace ProjectIchigo.Commands.Data;

internal class InfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var embeds = new List<DiscordEmbed>
            {
                new DiscordEmbedBuilder
                {
                    Title = "Privacy Policy for Ichigo",
                    Description = $"At Ichigo, accessible via this Discord Bot, one of our main priorities is the privacy of our visitors. This Privacy Policy document contains types of information that is collected and recorded by Ichigo and how we use it.\n" +
                    $"If you have additional questions or require more information about our Privacy Policy, do not hesitate to contact us.\n" +
                    $"This Privacy Policy applies only to our online activities and is valid for users of Ichigo with regards to the information that they shared and/or collect with Ichigo. Our Privacy Policy was created with the help of the TermsFeed Free Privacy Policy Generator.\n\n" +
                    $"**Consent**\n\n" +
                    $"By using this Discord Bot, you hereby consent to our Privacy Policy and agree to its terms.\n\n" +
                    $"**Information we collect**\n\n" +
                    $"The personal information that you are asked to provide, and the reasons why you have been asked to provide it, will be made clear to you at the point we ask you to provide your personal information.\n" +
                    $"If you contact us directly, we may receive additional information about you such as your name, email address, phone number, the contents of the message and/or attachments you may send us, and any other information you may choose to provide.\n\n" +
                    $"We use the information we collect in various ways, including to:\n" +
                    $"• Provide, operate, and maintain our Discord Bot\n" +
                    $"• Improve, personalize, and expand our Discord Bot\n" +
                    $"• Understand and analyze how you use our Discord Bot\n" +
                    $"• Develop new products, services, features, and functionality\n" +
                    $"• Communicate with you, either directly or through one of our partners, including for customer service, to provide you with updates and other information relating to Ichigo, and for marketing and promotional purposes\n" +
                    $"• Find and prevent fraud\n\n"
                }.SetInfo(ctx),
                new DiscordEmbedBuilder
                {
                    Description = $"**Log Files**\n\n" +
                    $"Ichigo follows a standard procedure of using log files. These files log usage of the Discord Bot. These are not linked to any information that is personally identifiable. The purpose of the information is for analyzing trends, administering Ichigo, tracking users' movement while actively using the Discord Bot, and gathering demographic information.\n\n" +
                    $"**Preferences**\n\n" +
                    $"Like most other Discord Bots, Ichigo will store your preferences.\n" +
                    $"You can choose to disable preferences stored by the bot via `{ctx.Prefix}data object`."
                }.SetInfo(ctx),
                new DiscordEmbedBuilder
                {
                    Description = $"**Advertising Partners Privacy Policies**\n\n" +
                    $"You may consult this list to find the Privacy Policy for each of the advertising partners of Ichigo.\n" +
                    $"Third-party ad servers or ad networks uses technologies like cookies, JavaScript, or Web Beacons that are used in their respective advertisements and links that appear on Ichigo, which are sent directly to users' browser. They automatically receive your IP address when this occurs. These technologies are used to measure the effectiveness of their advertising campaigns and/or to personalize the advertising content that you see on websites that you visit.\n" +
                    $"Note that Ichigo has no access to or control over these cookies that are used by third-party advertisers.\n\n" +
                    $"**Third Party Privacy Policies**\n\n" +
                    $"Ichigo's Privacy Policy does not apply to other advertisers or Discord Bots. Thus, we are advising you to consult the respective Privacy Policies of these third-party (ad) servers for more detailed information. It may include their practices and instructions about how to opt-out of certain options.\n\n" +
                    $"List of Third Party Privacy Policies:\n" +
                    $"• https://discord.com/privacy\n" +
                    $"• https://aitsys.dev/w/privacy"
                }.SetInfo(ctx),
                new DiscordEmbedBuilder
                {
                    Description = $"**CCPA Privacy Rights (Do Not Sell My Personal Information)**\n\n" +
                    $"Under the CCPA, among other rights, California consumers have the right to:\n" +
                    $"• Request that a business that collects a consumer's personal data disclose the categories and specific pieces of personal data that a business has collected about consumers.\n" +
                    $"• Request that a business delete any personal data about the consumer that a business has collected.\n" +
                    $"• Request that a business that sells a consumer's personal data, not sell the consumer's personal data.\n\n" +
                    $"If you make a request, we have one month to respond to you. If you would like to exercise any of these rights, please contact us.\n\n" +
                    $"**GDPR Data Protection Rights**\n\n" +
                    $"We would like to make sure you are fully aware of all of your data protection rights. Every user is entitled to the following:\n" +
                    $"• The right to access – You have the right to request copies of your personal data. We may charge you a small fee for this service.\r\n" +
                    $"• The right to rectification – You have the right to request that we correct any information you believe is inaccurate. You also have the right to request that we complete the information you believe is incomplete.\n" +
                    $"• The right to erasure – You have the right to request that we erase your personal data, under certain conditions.\n" +
                    $"• The right to restrict processing – You have the right to request that we restrict the processing of your personal data, under certain conditions.\n" +
                    $"• The right to object to processing – You have the right to object to our processing of your personal data, under certain conditions.\n" +
                    $"• The right to data portability – You have the right to request that we transfer the data that we have collected to another organization, or directly to you, under certain conditions.\n\n" +
                    $"If you make a request, we have one month to respond to you. If you would like to exercise any of these rights, please contact us.\n\n" +
                    $"**Children's Information**\n\n" +
                    $"Another part of our priority is adding protection for children while using the internet. We encourage parents and guardians to observe, participate in, and/or monitor and guide their online activity.\n" +
                    $"**Ichigo does not knowingly collect any Personal Identifiable Information from children under the age of 13. If you think that your child provided this kind of information on our plattform, we strongly encourage you to contact us immediately and we will do our best efforts to promptly remove such information from our records.**"
                }.SetInfo(ctx)
            };

            try
            {
                foreach (var b in embeds)
                    await ctx.Member.SendMessageAsync(b);

                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = ":mailbox_with_mail: `You got mail! Please check your DMs.`",
                }.SetSuccess(ctx));
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Description = "`It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                }.SetError(ctx);

                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                await RespondOrEdit(embed: errorembed);
            }
        });
    }
}