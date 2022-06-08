# Dynamics 365/Model-Driven Power Apps: Solving the AADSTS50011 Error
You may experience an uncommon, but frustrating and bewildering error while trying to access a Model-Driven interface in the Microsoft Dynamics 365/Power Platform:

```
Sorry, but we're having trouble signing you in.

AADSTS50011: The redirect URI 'https://by2--namcrmlivesg667.crm.dynamics.com/' specified in the request does not match the redirect URIs configured for the application '00000007-0000-0000-c000-000000000000'. Make sure the redirect URI sent in the request matches one added to your application in the Azure portal. Navigate to https://aka.ms/redirectUriMismatchError to learn more about how to fix this.
```

## When is this encountered?
This error would occur as soon as a user attempts to launch a model-driven interface, either in a Dynamics 365 first-party app or your own custom-built model-driven Power App.

## What does this mean?
There is a section within Azure Active Directory named *Enterprise Applications*. This contains a list of the applications that you have registered to be authenticated against and used within the context of M365. For example, if you wish for one of your applications to use the SharePoint API through the Microsoft Graph API, you'd register your app within Azure Active Directory here. The newly registered application would allow for you to set up an OAuth authentication flow, meaning users of your app would be able to log in with their own Microsoft account (personal or business) and in doing so allow your app to "use" SharePoint on their behalf. 

Upon the user authenticating (logging in), an expiring token is delivered back to *your* application via a simple GET request to your registered "callback" or "redirect" URL that you set up in the registered application in the Azure portal. This token is what you will provide to the Microsoft Graph API to verify that you were given permission by the user via authentication. 

**Application `00000007-0000-0000-c000-000000000000` (as seen in the error message above) is always the ID of the application used for authenticating into Dataverse**, formerly known as the Common Data Service. Speculating a bit here, what this error message *probably* means, is this:

- Every time a new environment is made, the platform will "set up" this new environment to work by providing it's unique callback address to the Dataverse application, `00000007-0000-0000-c000-000000000000`. Why? So after you log in as the user to use the environment, the model-driven app you intend to use will receive a unique token that will allow it to transact with Dataverse - perform reads, writes, updates, deletes, etc.
- For some reason, adding this new environment's callback URL is not working - The Dataverse Enterprise Application in the Azure portal does not have `https://by2--namcrmlivesg667.crm.dynamics.com/` registered as a redirect URL so therefore it denies the request. It does this to prevent a highly-sensitive access token from accidentally being sent to and received by someone who's hands it should not be in!

## What causes this? (speculation)
In my role, I am often having 15-30 users create an environment at the same time in the same tenant - between 15-30 distinct environments being created simultaneously! I have only seen these error occur in this scenario and see it occur in this scenario consistently, without exception.

Due to the fact that we are trying to add up to 30 callback/redirect URL's to the registered enterprise application `00000007-0000-0000-c000-000000000000` (Dataverse) all within 2-3 seconds, I would speculate that several are unsuccessfully being saved (registered properly), or are simply being overwritten by a concurrent registration.

I believe this to be the case for several reasons:
1. I've attempted this with many groups many times. It is a consistent issue and has happened each time.
2. The same result always occurs: roughly 50% of the group *does* have access to the model-driven interface as expected, but the other 50% *does not*. Some get through, some do not.
3. This error never occurs when slowing things down and only creating a new environment every 30 seconds to a minute or so. Perhaps one could create multiple environments even 5 seconds apart, but the nearly instantaneous creation is what seems to cause the error; not the quantity of the environments, but rather the time in between their creation.

## The solution
Fortunately, I have discovered what seems to be an indirect solution to this issue:

1. Give one of the affected users permission to use *another* environment in the tenant that *is* functioning properly (grant the Environment Maker role and perhaps a single Dataverse-related role for good measure).
2. The user selects (enters) the functioning environment they were given access to.
3. The user opens a model-driven app - *this step may not be necessary but has been followed in success scenarios*

That's it! Upon returning to their original environment, the user should find that the application now works as expected. They are no longer seeing the error message from above and can use their application (and any other model-driven interface) as expected. 

What's even better is that **now the error should be solved for ALL affected members of the tenant**. If there were 10 users suffering from this, the issue should no longer persist for anyone following one user completing the listed steps.